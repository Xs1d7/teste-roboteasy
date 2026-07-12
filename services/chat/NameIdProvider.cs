using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Api;

public class NameIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? connection.User?.FindFirstValue("sub");
}
