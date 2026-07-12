using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Services;

public class TokenService(IConfiguration config)
{
    public string CreateToken(User user)
    {
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key nao configurada");
        var issuer = config["Jwt:Issuer"] ?? "roboteasy-auth";
        var audience = config["Jwt:Audience"] ?? "roboteasy";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("username", user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var hours = int.TryParse(config["Jwt:ExpiresHours"], out var h) ? h : 12;

        var jwt = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddHours(hours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
