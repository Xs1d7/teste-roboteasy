using Chat.Api.Models;
using MongoDB.Bson;

namespace Chat.Api.Tests;

/// <summary>
/// Regressao do SendMessage: driver Mongo 3.x quebra Guid sem representation.
/// </summary>
public class ChatMessageSerializationTests
{
    [Fact]
    public void Guids_viram_string_no_bson()
    {
        var from = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var to = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var doc = new ChatMessage
        {
            FromUserId = from,
            ToUserId = to,
            FromUsername = "alice",
            ToUsername = "bob",
            Content = "oi"
        }.ToBsonDocument();

        Assert.Equal(BsonType.String, doc["FromUserId"].BsonType);
        Assert.Equal(BsonType.String, doc["ToUserId"].BsonType);
        Assert.Equal(from.ToString(), doc["FromUserId"].AsString);
        Assert.Equal(to.ToString(), doc["ToUserId"].AsString);
    }

    [Fact]
    public void Roundtrip_bson_mantem_guids()
    {
        var original = new ChatMessage
        {
            FromUserId = Guid.NewGuid(),
            ToUserId = Guid.NewGuid(),
            FromUsername = "a",
            ToUsername = "b",
            Content = "ping",
            SentAt = DateTime.UtcNow
        };

        var bytes = original.ToBson();
        var restored = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<ChatMessage>(bytes);

        Assert.Equal(original.FromUserId, restored.FromUserId);
        Assert.Equal(original.ToUserId, restored.ToUserId);
        Assert.Equal(original.Content, restored.Content);
    }
}
