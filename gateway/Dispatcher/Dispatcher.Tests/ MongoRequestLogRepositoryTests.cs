using Dispatcher.Domain.Logging;
using Dispatcher.Infrastructure.Logging;
using FluentAssertions;
using Mongo2Go;
using MongoDB.Driver;

namespace Dispatcher.Tests;

public class MongoRequestLogRepositoryTests : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly IMongoDatabase _database;

    public MongoRequestLogRepositoryTests()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("dispatcher-log-test-db");
    }

    [Fact]
    public async Task AddAsync_And_GetRecentAsync_Should_Persist_Request_Log()
    {
        var repository = new MongoRequestLogRepository(_database);

        await repository.AddAsync(new RequestLog
        {
            Id = "log-1",
            TimestampUtc = DateTime.UtcNow,
            Path = "/api/flights",
            Method = "GET",
            StatusCode = 200,
            DurationMs = 12.5,
            UserId = "admin-1",
            UserRole = "Admin",
            TargetService = "FlightService"
        });

        var result = await repository.GetRecentAsync();

        result.Should().ContainSingle();
        result[0].Id.Should().Be("log-1");
        result[0].UserId.Should().Be("admin-1");
        result[0].UserRole.Should().Be("Admin");
        result[0].TargetService.Should().Be("FlightService");
        result[0].ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task GetRecentAsync_Should_Return_Newest_First()
    {
        var repository = new MongoRequestLogRepository(_database);

        await repository.AddAsync(new RequestLog
        {
            Id = "log-older",
            TimestampUtc = DateTime.UtcNow.AddMinutes(-5),
            Path = "/api/auth/login",
            Method = "POST",
            StatusCode = 200,
            DurationMs = 5
        });

        await repository.AddAsync(new RequestLog
        {
            Id = "log-newer",
            TimestampUtc = DateTime.UtcNow,
            Path = "/api/flights",
            Method = "GET",
            StatusCode = 502,
            DurationMs = 18,
            TargetService = "FlightService",
            ErrorMessage = "Bad Gateway"
        });

        var result = await repository.GetRecentAsync(2);

        result.Select(x => x.Id).Should().ContainInOrder("log-newer", "log-older");
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}
