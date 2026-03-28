using Dispatcher.Application.Routing;
using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Routing;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dispatcher.Tests;

public class DatabaseRouteResolverTests
{
    [Fact]
    public async Task ResolveAsync_Should_Return_Route_When_Repository_Finds_Match()
    {
        // Arrange
        var expectedRoute = new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = "/api/flights",
            HttpMethod = "GET",
            TargetServiceName = "FlightService",
            TargetBaseUrl = "http://flightservice:5002",
            RequiresAuth = true,
            AllowedRoles = new List<string> { "Admin", "User" }
        };

        var repositoryMock = new Mock<IRouteRepository>();
        repositoryMock
            .Setup(x => x.FindRouteAsync("/api/flights", "GET"))
            .ReturnsAsync(expectedRoute);

        var resolver = new DatabaseRouteResolver(repositoryMock.Object);

        // Act
        var result = await resolver.ResolveAsync("/api/flights", "GET");

        // Assert
        result.Should().NotBeNull();
        result!.PathPrefix.Should().Be("/api/flights");
        result.HttpMethod.Should().Be("GET");
        result.TargetServiceName.Should().Be("FlightService");
    }

    [Fact]
    public async Task ResolveAsync_Should_Return_Null_When_Repository_Finds_No_Match()
    {
        // Arrange
        var repositoryMock = new Mock<IRouteRepository>();
        repositoryMock
            .Setup(x => x.FindRouteAsync("/api/unknown", "GET"))
            .ReturnsAsync((RouteDefinition?)null);

        var resolver = new DatabaseRouteResolver(repositoryMock.Object);

        // Act
        var result = await resolver.ResolveAsync("/api/unknown", "GET");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_Should_Return_Null_For_Empty_Path()
    {
        // Arrange
        var repositoryMock = new Mock<IRouteRepository>();
        var resolver = new DatabaseRouteResolver(repositoryMock.Object);

        // Act
        var result = await resolver.ResolveAsync(string.Empty, "GET");

        // Assert
        result.Should().BeNull();
    }
}