using System.Security.Claims;
using Chat.Api.Models;
using Chat.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class ChatController(IPresenceTracker presence, MessageStore store) : ControllerBase
{
    [HttpGet("users/online")]
    public ActionResult<IEnumerable<OnlineUser>> Online() => Ok(presence.GetOnline());

    [HttpGet("messages")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> History([FromQuery] Guid with)
    {
        var me = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (me is null || !Guid.TryParse(me, out var myId))
            return Unauthorized();

        var msgs = await store.GetConversationAsync(myId, with);
        msgs.Reverse();

        return Ok(msgs.Select(m => new MessageDto(
            m.Id!, m.FromUserId, m.FromUsername, m.ToUserId, m.ToUsername, m.Content, m.SentAt)));
    }
}
