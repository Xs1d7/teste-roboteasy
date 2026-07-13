using System.Security.Claims;
using Chat.Api.Models;
using Chat.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Api.Hubs;

[Authorize]
public class ChatHub(
    IPresenceTracker presence,
    PresenceConnectionRegistry connections,
    MessageStore store,
    EventBus bus) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = CurrentUser();
        if (user is null)
        {
            Context.Abort();
            return;
        }

        connections.Register(Context.ConnectionId);

        if (presence.TryAdd(Context.ConnectionId, user))
        {
            await bus.PublishAsync(new ChatEvent
            {
                Type = "user.online",
                UserId = user.UserId,
                Username = user.Username
            });
        }

        await Clients.Caller.SendAsync("OnlineUsers", presence.GetOnline());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        connections.Unregister(Context.ConnectionId);

        if (presence.TryRemove(Context.ConnectionId, out var user) && user is not null)
        {
            await bus.PublishAsync(new ChatEvent
            {
                Type = "user.offline",
                UserId = user.UserId,
                Username = user.Username
            });
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid toUserId, string toUsername, string content)
    {
        var from = CurrentUser() ?? throw new HubException("nao autenticado");

        content = (content ?? string.Empty).Trim();
        if (content.Length == 0) throw new HubException("mensagem vazia");
        if (content.Length > 2000) content = content[..2000];

        var saved = await store.SaveAsync(new ChatMessage
        {
            FromUserId = from.UserId,
            FromUsername = from.Username,
            ToUserId = toUserId,
            ToUsername = toUsername,
            Content = content,
            SentAt = DateTime.UtcNow
        });

        await bus.PublishAsync(new ChatEvent
        {
            Type = "message",
            Message = new MessageDto(
                saved.Id!,
                saved.FromUserId,
                saved.FromUsername,
                saved.ToUserId,
                saved.ToUsername,
                saved.Content,
                saved.SentAt)
        });
    }

    /// <summary>Ephemeral: notifica o peer que o usuario esta digitando (sem Rabbit/Mongo).</summary>
    public Task Typing(Guid toUserId, bool isTyping)
    {
        var from = CurrentUser() ?? throw new HubException("nao autenticado");
        return Clients.User(toUserId.ToString()).SendAsync("UserTyping", new
        {
            userId = from.UserId,
            username = from.Username,
            isTyping
        });
    }

    private OnlineUser? CurrentUser()
    {
        var id = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");
        var name = Context.User?.FindFirstValue(ClaimTypes.Name)
            ?? Context.User?.FindFirstValue("username")
            ?? Context.User?.Identity?.Name;

        if (id is null || name is null || !Guid.TryParse(id, out var userId))
            return null;

        return new OnlineUser(userId, name);
    }
}
