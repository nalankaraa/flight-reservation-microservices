using Mongo2Go;
using MongoDB.Driver;

namespace AuthService.Tests;

public sealed class MongoUserRepositoryTestFixture : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly MongoClient _client;

    public MongoUserRepositoryTestFixture()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
        _client = new MongoClient(_runner.ConnectionString);
    }

    public IMongoDatabase CreateDatabase()
    {
        return _client.GetDatabase($"auth-tests-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}