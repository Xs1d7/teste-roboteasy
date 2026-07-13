using Auth.Api.Controllers;
using Auth.Api.Data;
using Auth.Api.Dtos;
using Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Auth.Api.Tests;

public class AuthControllerTests
{
    private static (AuthController Ctrl, AppDbContext Db) CreateSut()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"auth_test_{Guid.NewGuid():N}")
            .Options;

        var db = new AppDbContext(options);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-roboteasy-at-least-32-chars!",
                ["Jwt:Issuer"] = "roboteasy-auth",
                ["Jwt:Audience"] = "roboteasy",
                ["Jwt:ExpiresHours"] = "1"
            })
            .Build();

        return (new AuthController(db, new TokenService(config)), db);
    }

    [Fact]
    public async Task Register_cria_usuario_e_retorna_jwt()
    {
        var (ctrl, db) = CreateSut();

        var result = await ctrl.Register(new RegisterRequest("alice", "senha123"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("alice", body.Username);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.NotEqual(Guid.Empty, body.UserId);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Register_username_duplicado_retorna_conflict()
    {
        var (ctrl, _) = CreateSut();
        await ctrl.Register(new RegisterRequest("bob", "senha123"));

        var result = await ctrl.Register(new RegisterRequest("bob", "outra"));

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflict.StatusCode);
    }

    [Fact]
    public async Task Login_credenciais_validas_retorna_jwt()
    {
        var (ctrl, _) = CreateSut();
        await ctrl.Register(new RegisterRequest("carol", "senha123"));

        var result = await ctrl.Login(new LoginRequest("carol", "senha123"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("carol", body.Username);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
    }

    [Fact]
    public async Task Login_senha_errada_retorna_unauthorized()
    {
        var (ctrl, _) = CreateSut();
        await ctrl.Register(new RegisterRequest("dave", "senha123"));

        var result = await ctrl.Login(new LoginRequest("dave", "errada"));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal(401, unauthorized.StatusCode);
    }

    [Fact]
    public async Task Register_username_curto_retorna_bad_request()
    {
        var (ctrl, _) = CreateSut();

        var result = await ctrl.Register(new RegisterRequest("ab", "senha123"));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
