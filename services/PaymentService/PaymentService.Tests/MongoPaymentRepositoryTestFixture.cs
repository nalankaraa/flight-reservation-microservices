using Mongo2Go;
using MongoDB.Driver;

namespace PaymentService.Tests;

public sealed class MongoPaymentRepositoryTestFixture : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly MongoClient _client;

    public MongoPaymentRepositoryTestFixture()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
        _client = new MongoClient(_runner.ConnectionString);
    }

    public IMongoDatabase CreateDatabase()
    {
        return _client.GetDatabase($"payment-tests-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}