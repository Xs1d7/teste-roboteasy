using System.Collections.Concurrent;
using Chat.Api.Models;

namespace Chat.Api.Services;

public class PresenceTracker
{
    private readonly ConcurrentDictionary<string, OnlineUser> _byConnection = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _byUser = new();

    public bool TryAdd(string connectionId, OnlineUser user)
    {
        _byConnection[connectionId] = user;
        var conns = _byUser.GetOrAdd(user.UserId, _ => new ConcurrentDictionary<string, byte>());
        conns[connectionId] = 0;
        return conns.Count == 1;
    }

    public bool TryRemove(string connectionId, out OnlineUser? user)
    {
        user = null;
        if (!_byConnection.TryRemove(connectionId, out var found))
            return false;

        user = found;
        if (_byUser.TryGetValue(found.UserId, out var conns))
        {
            conns.TryRemove(connectionId, out _);
            if (conns.IsEmpty)
            {
                _byUser.TryRemove(found.UserId, out _);
                return true;
            }
        }

        return false;
    }

    public IReadOnlyList<OnlineUser> GetOnline()
    {
        return _byUser.Keys
            .Select(id => _byConnection.Values.FirstOrDefault(u => u.UserId == id) ?? new OnlineUser(id, "?"))
            .OrderBy(u => u.Username)
            .ToList();
    }
}
