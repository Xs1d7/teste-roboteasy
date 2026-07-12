using Chat.Api.Models;
using Chat.Api.Services;

namespace Chat.Api.Tests;

public class PresenceTrackerTests
{
    [Fact]
    public void Multi_aba_so_fica_offline_no_ultimo_disconnect()
    {
        var tracker = new PresenceTracker();
        var user = new OnlineUser(Guid.NewGuid(), "alice");

        Assert.True(tracker.TryAdd("c1", user));
        Assert.False(tracker.TryAdd("c2", user));

        Assert.False(tracker.TryRemove("c1", out _));
        Assert.True(tracker.TryRemove("c2", out var removed));
        Assert.Equal(user.UserId, removed!.UserId);
        Assert.Empty(tracker.GetOnline());
    }

    [Fact]
    public void Lista_online_nao_inclui_quem_saiu()
    {
        var tracker = new PresenceTracker();
        var a = new OnlineUser(Guid.NewGuid(), "a");
        var b = new OnlineUser(Guid.NewGuid(), "b");

        tracker.TryAdd("1", a);
        tracker.TryAdd("2", b);
        tracker.TryRemove("1", out _);

        var online = tracker.GetOnline();
        Assert.Single(online);
        Assert.Equal(b.UserId, online[0].UserId);
    }
}
