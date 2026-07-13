using System.Text.Json;
using Chat.Api.Models;
using StackExchange.Redis;

namespace Chat.Api.Services;

/// <summary>
/// Presenca compartilhada via Redis — necessaria quando o Chat roda com N instancias.
/// </summary>
public class RedisPresenceTracker(IConnectionMultiplexer mux) : IPresenceTracker
{
    private const string ConnKey = "roboteasy:presence:conn";
    private const string UsersKey = "roboteasy:presence:users";
    private static string UserConnsKey(Guid userId) => $"roboteasy:presence:user:{userId:D}";

    private readonly IDatabase _db = mux.GetDatabase();

    public bool TryAdd(string connectionId, OnlineUser user)
    {
        var payload = JsonSerializer.Serialize(new ConnPayload(user.UserId, user.Username));
        _db.HashSet(ConnKey, connectionId, payload);
        _db.HashSet(UsersKey, user.UserId.ToString("D"), user.Username);

        _db.SetAdd(UserConnsKey(user.UserId), connectionId);
        return _db.SetLength(UserConnsKey(user.UserId)) == 1;
    }

    public bool TryRemove(string connectionId, out OnlineUser? user)
    {
        user = null;
        var raw = _db.HashGet(ConnKey, connectionId);
        if (raw.IsNullOrEmpty)
            return false;

        _db.HashDelete(ConnKey, connectionId);
        var payload = JsonSerializer.Deserialize<ConnPayload>(raw.ToString());
        if (payload is null)
            return false;

        user = new OnlineUser(payload.UserId, payload.Username);
        var userKey = UserConnsKey(payload.UserId);
        _db.SetRemove(userKey, connectionId);

        if (_db.SetLength(userKey) == 0)
        {
            _db.KeyDelete(userKey);
            _db.HashDelete(UsersKey, payload.UserId.ToString("D"));
            return true;
        }

        return false;
    }

    public IReadOnlyList<OnlineUser> GetOnline()
    {
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

    private sealed record ConnPayload(Guid UserId, string Username);
}
