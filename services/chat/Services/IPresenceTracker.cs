using Chat.Api.Models;

namespace Chat.Api.Services;

public interface IPresenceTracker
{
    bool TryAdd(string connectionId, OnlineUser user);
    bool TryRemove(string connectionId, out OnlineUser? user);
    IReadOnlyList<OnlineUser> GetOnline();
}
