using Mongo2Go;
using MongoDB.Driver;

namespace AvailabilityService.Tests;

public sealed class MongoSeatHoldRepositoryTestFixture : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly MongoClient _client;

    public MongoSeatHoldRepositoryTestFixture()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
        _client = new MongoClient(_runner.ConnectionString);
    }

    public IMongoDatabase CreateDatabase()
    {
        return _client.GetDatabase($"availability-tests-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}