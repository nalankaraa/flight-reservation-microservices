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
    private readonly IMongoDatabase _database;

    public MongoRouteRepositoryTests()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("dispatcher-test-db");
    }

    [Fact]
    public async Task FindRouteAsync_KnownPathAndMethod_ReturnsRoute()
    {
        // Arrange
        var repository = new MongoRouteRepository(_database);

        await repository.AddRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = "/api/flights",
            HttpMethod = "GET",
            TargetServiceName = "FlightService",
            TargetBaseUrl = "http://localhost:5162",
            RequiresAuth = true,
            AllowedRoles = new List<string> { "Admin", "Customer" }
        });

        // Act
        var result = await repository.FindRouteAsync("/api/flights", "GET");

        // Assert
        result.Should().NotBeNull();
        result!.PathPrefix.Should().Be("/api/flights");
        result.HttpMethod.Should().Be("GET");
        result.TargetServiceName.Should().Be("FlightService");
        result.TargetBaseUrl.Should().Be("http://localhost:5162");
        result.RequiresAuth.Should().BeTrue();
        result.AllowedRoles.Should().Contain(new[] { "Admin", "Customer" });
    }

    [Fact]
    public async Task FindRouteAsync_UnknownPath_ReturnsNull()
    {
        // Arrange
        var repository = new MongoRouteRepository(_database);

        // Act
        var result = await repository.FindRouteAsync("/api/unknown", "GET");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindRouteAsync_MethodMismatch_ReturnsNull()
    {
        // Arrange
        var repository = new MongoRouteRepository(_database);

        await repository.AddRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = "/api/flights",
            HttpMethod = "GET",
            TargetServiceName = "FlightService",
            TargetBaseUrl = "http://localhost:5162",
            RequiresAuth = true,
            AllowedRoles = new List<string> { "Admin", "Customer" }
        });

        // Act
        var result = await repository.FindRouteAsync("/api/flights", "POST");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpsertRouteAsync_ExistingRoute_UpdatesTargetBaseUrl()
    {
        var repository = new MongoRouteRepository(_database);

        await repository.AddRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = "/api/flights",
            HttpMethod = "GET",
            TargetServiceName = "FlightService",
            TargetBaseUrl = "http://localhost:5162",
            RequiresAuth = true,
            AllowedRoles = new List<string> { "Admin", "Customer" }
        });

        await repository.UpsertRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = "/api/flights",
            HttpMethod = "GET",
            TargetServiceName = "FlightService",
            TargetBaseUrl = "http://flightservice:8080",
            RequiresAuth = true,
            AllowedRoles = new List<string> { "Admin", "Customer" }
        });

        var result = await repository.FindRouteAsync("/api/flights", "GET");

        result.Should().NotBeNull();
        result!.TargetBaseUrl.Should().Be("http://flightservice:8080");
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}