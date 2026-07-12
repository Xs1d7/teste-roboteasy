using System.Text;
using System.Text.Json;
using Chat.Api.Models;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Chat.Api.Services;

public class EventConsumer(
    EventBus bus,
    IHubContext<Hubs.ChatHub> hub,
    ILogger<EventConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queue = await bus.Channel.QueueDeclareAsync(
            queue: string.Empty,
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: stoppingToken);

        await bus.Channel.QueueBindAsync(queue.QueueName, EventBus.ExchangeName, string.Empty, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(bus.Channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var evt = JsonSerializer.Deserialize<ChatEvent>(Encoding.UTF8.GetString(args.Body.ToArray()));
                if (evt is null) return;

                if (evt.Type == "message" && evt.Message is not null)
                {
                    await hub.Clients.User(evt.Message.ToUserId.ToString())
                        .SendAsync("ReceiveMessage", evt.Message, stoppingToken);
                    await hub.Clients.User(evt.Message.FromUserId.ToString())
                        .SendAsync("ReceiveMessage", evt.Message, stoppingToken);
                }
                else if (evt.Type is "user.online" or "user.offline")
                {
                    await hub.Clients.All.SendAsync("PresenceChanged", new
                    {
                        type = evt.Type,
                        userId = evt.UserId,
                        username = evt.Username
                    }, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "erro no consumer rabbit");
            }
        };

        await bus.Channel.BasicConsumeAsync(queue.QueueName, autoAck: true, consumer, stoppingToken);
        logger.LogInformation("consumindo fila {Queue}", queue.QueueName);

        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { }
    }
}
