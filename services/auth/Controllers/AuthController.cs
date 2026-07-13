using Auth.Api.Data;
using Auth.Api.Dtos;
using Auth.Api.Models;
using Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Username e senha obrigatorios." });

        if (req.Username.Trim().Length < 3 || req.Password.Length < 4)
            return BadRequest(new { message = "Username min 3, senha min 4." });

        var name = req.Username.Trim();
        if (await db.Users.AnyAsync(u => u.Username == name))
            return Conflict(new { message = "Username ja existe." });

        var user = new User
        {
            Username = name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new AuthResponse(tokens.CreateToken(user), user.Id, user.Username, null));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Usuário ou senha inválidos." });

        // URL relativa: funciona via proxy do frontend (Vite/nginx) sem hardcode de host
        string? avatarUrl = string.IsNullOrEmpty(user.AvatarKey)
            ? null
            : $"/api/users/avatar/{user.AvatarKey}";

        var token = tokens.CreateToken(user);

        return Ok(new AuthResponse(token, user.Id, user.Username, avatarUrl));
    }
}
