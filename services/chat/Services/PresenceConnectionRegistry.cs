using System.Collections.Concurrent;

namespace Chat.Api.Services;

/// <summary>
/// Conexoes SignalR ativas neste processo — para renovar TTL no Redis mesmo com o usuario ocioso.
/// </summary>
public class PresenceConnectionRegistry
{
    private readonly ConcurrentDictionary<string, byte> _ids = new();

    public void Register(string connectionId) => _ids[connectionId] = 0;

    public void Unregister(string connectionId) => _ids.TryRemove(connectionId, out _);

    public IReadOnlyCollection<string> Snapshot() => _ids.Keys.ToArray();
}
