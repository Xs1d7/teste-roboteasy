using Microsoft.AspNetCore.SignalR;

namespace Chat.Api.Services;

/// <summary>Renova TTL de presenca a cada invocacao no hub (SendMessage, etc.).</summary>
public class PresenceHeartbeatFilter(IPresenceTracker presence) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        presence.Heartbeat(invocationContext.Context.ConnectionId);
        return await next(invocationContext);
    }
}
