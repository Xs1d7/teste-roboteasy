using Chat.Api.Models;
using Chat.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Chat.Api.Tests;

public class MessageStoreTests
{
    private static string? MongoUrl =>
        Environment.GetEnvironmentVariable("MONGO_TEST_URL")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__Mongo");

    [Fact]
    public async Task Save_e_busca_conversa_com_guid()
    {
        var url = MongoUrl;
        if (string.IsNullOrWhiteSpace(url))
            return;

        var mongo = url.Contains('?')
            ? $"{url}&serverSelectionTimeoutMS=3000"
            : $"{url}/?serverSelectionTimeoutMS=3000";

        var dbName = $"roboteasy_test_{Guid.NewGuid():N}";
        MessageStore store;
        try
        {
            store = new MessageStore(new TinyConfig(mongo, dbName));
        }
        catch (Exception ex) when (ex is TimeoutException or MongoDB.Driver.MongoConnectionException)
        {
            return;
        }

        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();

        var saved = await store.SaveAsync(new ChatMessage
        {
            FromUserId = alice,
            FromUsername = "alice",
            ToUserId = bob,
            ToUsername = "bob",
            Content = "automated",
            SentAt = DateTime.UtcNow
        });

        Assert.False(string.IsNullOrWhiteSpace(saved.Id));

        var history = await store.GetConversationAsync(alice, bob);
        Assert.Contains(history, m => m.Content == "automated" && m.FromUserId == alice);
    }

    private sealed class TinyConfig(string mongoUrl, string database) : IConfiguration
    {
        public string? this[string key]
        {
            get => key switch
            {
                "ConnectionStrings:Mongo" => mongoUrl,
                "Mongo:Database" => database,
                _ => null
            };
            set { }
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => new CancellationChangeToken(CancellationToken.None);
        public IConfigurationSection GetSection(string key) => new TinySection(this, key);
    }

    private sealed class TinySection(IConfiguration root, string path) : IConfigurationSection
    {
        public string? this[string key]
        {
            get => root[$"{path}:{key}"];
            set { }
        }

        public string Key => path.Contains(':') ? path[(path.LastIndexOf(':') + 1)..] : path;
        public string Path => path;
        public string? Value
        {
            get => root[path];
            set { }
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => new CancellationChangeToken(CancellationToken.None);
        public IConfigurationSection GetSection(string key) => new TinySection(root, $"{path}:{key}");
    }
}
