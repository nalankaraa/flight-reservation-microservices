using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Routing;
using FluentAssertions;
using Mongo2Go;
using MongoDB.Driver;
using Xunit;
namespace Dispatcher.Tests;
public class MongoRouteRepositoryTests : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly MongoRouteRepository _repository;
    public MongoRouteRepositoryTests()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase("dispatcher-test-db");
        _repository = new MongoRouteRepository(database);  // henüz yok → derlenmez
    }
    [Fact]
    public async Task FindRouteAsync_KnownPathAndMethod_ReturnsRoute()
    {
        await _repository.AddRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = "/api/flights",
            HttpMethod = "GET",
            TargetServiceName = "FlightService",
            TargetBaseUrl = "http://flightservice:5002",
            RequiresAuth = true,
            AllowedRoles = new List<string> { "Admin", "User" }
        });
        var result = await _repository.FindRouteAsync("/api/flights", "GET");
        result.Should().NotBeNull();
        result!.PathPrefix.Should().Be("/api/flights");
        result.TargetServiceName.Should().Be("FlightService");
    }
    [Fact]
    public async Task FindRouteAsync_UnknownPath_ReturnsNull()
    {
        var result = await _repository.FindRouteAsync("/api/unknown", "GET");
        result.Should().BeNull();
    }

    public void Dispose() => _runner.Dispose();
}