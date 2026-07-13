namespace Auth.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
