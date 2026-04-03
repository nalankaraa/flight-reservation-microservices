using Mongo2Go;
using MongoDB.Driver;

namespace NotificationService.Tests;

public sealed class MongoNotificationRepositoryTestFixture : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly MongoClient _client;

    public MongoNotificationRepositoryTestFixture()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
        _client = new MongoClient(_runner.ConnectionString);
    }

    public IMongoDatabase CreateDatabase()
    {
        return _client.GetDatabase($"notification-tests-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}