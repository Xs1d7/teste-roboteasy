namespace Auth.Api.Dtos;

public record RegisterRequest(string Username, string Password);
public record LoginRequest(string Username, string Password);
public record AuthResponse(string Token, Guid UserId, string Username, string? AvatarUrl);
public record UserDto(Guid Id, string Username);
