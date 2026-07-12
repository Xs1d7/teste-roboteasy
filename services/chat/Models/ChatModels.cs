using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chat.Api.Models;

public class ChatMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // driver 3.x exige representation explicita pra Guid
    [BsonRepresentation(BsonType.String)]
    public Guid FromUserId { get; set; }
    public string FromUsername { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public Guid ToUserId { get; set; }
    public string ToUsername { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

public record OnlineUser(Guid UserId, string Username);

public record MessageDto(
    string Id,
    Guid FromUserId,
    string FromUsername,
    Guid ToUserId,
    string ToUsername,
    string Content,
    DateTime SentAt);

public class ChatEvent
{
    public string Type { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public MessageDto? Message { get; set; }
}
