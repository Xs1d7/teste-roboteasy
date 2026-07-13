using System.Text.Json;
using Chat.Api.Models;
using StackExchange.Redis;

namespace Chat.Api.Services;

/// <summary>
/// Presenca compartilhada via Redis com TTL por conexao.
/// Heartbeat renova o TTL; se o pod morrer sem disconnect, a chave expira e o usuario some do online.
/// </summary>
public class RedisPresenceTracker(IConnectionMultiplexer mux) : IPresenceTracker
{
    public static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);

    private const string UsersKey = "roboteasy:presence:users";
    private static string ConnKey(string connectionId) => $"roboteasy:presence:conn:{connectionId}";
    private static string UserConnsKey(Guid userId) => $"roboteasy:presence:user:{userId:D}";

    private readonly IDatabase _db = mux.GetDatabase();

    public bool TryAdd(string connectionId, OnlineUser user)
    {
        var payload = JsonSerializer.Serialize(new ConnPayload(user.UserId, user.Username));
        _db.StringSet(ConnKey(connectionId), payload, Ttl);
        _db.HashSet(UsersKey, user.UserId.ToString("D"), user.Username);
        _db.SetAdd(UserConnsKey(user.UserId), connectionId);
        return _db.SetLength(UserConnsKey(user.UserId)) == 1;
    }

    public bool TryRemove(string connectionId, out OnlineUser? user)
    {
        user = null;
        var key = ConnKey(connectionId);
        var raw = _db.StringGet(key);
        if (raw.IsNullOrEmpty)
            return false;

        _db.KeyDelete(key);
        var payload = JsonSerializer.Deserialize<ConnPayload>(raw.ToString());
        if (payload is null)
            return false;

        user = new OnlineUser(payload.UserId, payload.Username);
        return RemoveConnectionFromUser(payload.UserId, connectionId);
    }

    public void Heartbeat(string connectionId)
    {
        var key = ConnKey(connectionId);
        if (_db.KeyExists(key))
            _db.KeyExpire(key, Ttl);
    }

    public IReadOnlyList<OnlineUser> GetOnline()
    {
        PruneExpired();

        var entries = _db.HashGetAll(UsersKey);
        return entries
            .Select(e =>
            {
                Guid.TryParse(e.Name.ToString(), out var id);
                return new OnlineUser(id, e.Value.ToString());
            })
            .Where(u => u.UserId != Guid.Empty)
            .OrderBy(u => u.Username)
            .ToList();
    }

    /// <summary>Remove conexoes cujo TTL ja expirou (pod morto / WS abandonado).</summary>
    public void PruneExpired()
    {
        var users = _db.HashGetAll(UsersKey);
        foreach (var entry in users)
        {
            if (!Guid.TryParse(entry.Name.ToString(), out var userId))
                continue;

            var userKey = UserConnsKey(userId);
            var conns = _db.SetMembers(userKey);
            foreach (var conn in conns)
            {
                var connId = conn.ToString();
                if (!_db.KeyExists(ConnKey(connId)))
                    _db.SetRemove(userKey, connId);
            }

            if (_db.SetLength(userKey) == 0)
            {
                _db.KeyDelete(userKey);
                _db.HashDelete(UsersKey, userId.ToString("D"));
            }
        }
    }

    private bool RemoveConnectionFromUser(Guid userId, string connectionId)
    {
        var userKey = UserConnsKey(userId);
        _db.SetRemove(userKey, connectionId);

        if (_db.SetLength(userKey) == 0)
        {
            _db.KeyDelete(userKey);
            _db.HashDelete(UsersKey, userId.ToString("D"));
            return true;
        }

        return false;
    }

    private sealed record ConnPayload(Guid UserId, string Username);
}
