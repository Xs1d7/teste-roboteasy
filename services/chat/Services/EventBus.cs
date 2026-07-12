using System.Text;
using System.Text.Json;
using Chat.Api.Models;
using RabbitMQ.Client;

namespace Chat.Api.Services;

public class EventBus : IAsyncDisposable
{
    public const string ExchangeName = "chat.events";

    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<EventBus> _logger;

    private EventBus(IConnection connection, IChannel channel, ILogger<EventBus> logger)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
    }

    public static async Task<EventBus> CreateAsync(IConfiguration config, ILogger<EventBus> logger)
    {
        var uri = config.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672";
        Exception? last = null;

        for (var i = 0; i < 12; i++)
        {
            try
            {
                var factory = new ConnectionFactory { Uri = new Uri(uri) };
                var conn = await factory.CreateConnectionAsync();
                var channel = await conn.CreateChannelAsync();
                await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Fanout, durable: true);
                logger.LogInformation("RabbitMQ ok");
                return new EventBus(conn, channel, logger);
            }
            catch (Exception ex)
            {
                last = ex;
                logger.LogWarning("RabbitMQ ainda nao subiu ({Attempt}/12)", i + 1);
                await Task.Delay(2500);
            }
        }

        throw new InvalidOperationException("Falha ao conectar no RabbitMQ", last);
    }

    public IChannel Channel => _channel;

    public async Task PublishAsync(ChatEvent evt)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
        await _channel.BasicPublishAsync(ExchangeName, routingKey: string.Empty, body: body);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "erro fechando rabbit");
        }
    }
}
