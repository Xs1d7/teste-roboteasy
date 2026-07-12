using Chat.Api.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Chat.Api.Services;

public class MessageStore
{
    private static int _configured;
    private readonly IMongoCollection<ChatMessage> _messages;

    public MessageStore(IConfiguration config)
    {
        if (Interlocked.Exchange(ref _configured, 1) == 0)
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

        var client = new MongoClient(config.GetConnectionString("Mongo"));
        var db = client.GetDatabase(config["Mongo:Database"] ?? "roboteasy_chat");
        _messages = db.GetCollection<ChatMessage>("messages");

        var keys = Builders<ChatMessage>.IndexKeys
            .Ascending(m => m.FromUserId)
            .Ascending(m => m.ToUserId)
            .Ascending(m => m.SentAt);
        _messages.Indexes.CreateOne(new CreateIndexModel<ChatMessage>(keys));
    }

    public async Task<ChatMessage> SaveAsync(ChatMessage msg)
    {
        await _messages.InsertOneAsync(msg);
        return msg;
    }

    public async Task<List<ChatMessage>> GetConversationAsync(Guid a, Guid b, int limit = 100)
    {
        var filter = Builders<ChatMessage>.Filter.Or(
            Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.FromUserId, a),
                Builders<ChatMessage>.Filter.Eq(m => m.ToUserId, b)),
            Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.FromUserId, b),
                Builders<ChatMessage>.Filter.Eq(m => m.ToUserId, a)));

        return await _messages.Find(filter).SortByDescending(m => m.SentAt).Limit(limit).ToListAsync();
    }
}
