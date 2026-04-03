using Mongo2Go;
using MongoDB.Driver;

namespace ReservationService.Tests;

public sealed class MongoReservationTestFixture : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly MongoClient _client;

    public MongoReservationTestFixture()
    {
        _runner = MongoDbRunner.Start();
        _client = new MongoClient(_runner.ConnectionString);
    }

    public IMongoDatabase CreateDatabase()
    {
        return _client.GetDatabase($"reservation-test-db-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}
