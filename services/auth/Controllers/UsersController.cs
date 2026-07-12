using Auth.Api.Data;
using Auth.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(AppDbContext db) : ControllerBase
{
    // lista cadastrados — online de verdade fica no Chat
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> List()
    {
        var list = await db.Users
            .OrderBy(u => u.Username)
            .Select(u => new UserDto(u.Id, u.Username))
            .ToListAsync();

        return Ok(list);
    }
}
