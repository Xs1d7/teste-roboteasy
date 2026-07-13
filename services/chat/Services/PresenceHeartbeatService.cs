namespace Chat.Api.Services;

/// <summary>
/// Renova TTL das conexoes locais a cada 20s — cobre usuario ocioso (sem SendMessage).
/// TTL Redis = 60s; se o processo morrer, as chaves expiram sem heartbeat.
/// </summary>
public class PresenceHeartbeatService(
    PresenceConnectionRegistry registry,
    IPresenceTracker presence,
    ILogger<PresenceHeartbeatService> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(20));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var ids = registry.Snapshot();
            foreach (var id in ids)
                presence.Heartbeat(id);

            if (ids.Count > 0)
                log.LogDebug("presence heartbeat em {Count} conexoes", ids.Count);

            if (presence is RedisPresenceTracker redis)
                redis.PruneExpired();
        }
    }
}
