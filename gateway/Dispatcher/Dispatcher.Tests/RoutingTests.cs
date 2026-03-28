using Dispatcher.Infrastructure.Routing;
using FluentAssertions;
using Xunit;

namespace Dispatcher.Tests;

public class RoutingTests
{
    [Fact]
    public async Task ResolveAsync_Should_Return_FlightService_Route_For_GetFlights()
    {
        // Arrange
        var resolver = new InMemoryRouteResolver();

        // Act
        var route = await resolver.ResolveAsync("/api/flights", "GET");

        // Assert
        route.Should().NotBeNull();
        route!.PathPrefix.Should().Be("/api/flights");
        route.HttpMethod.Should().Be("GET");
        route.TargetServiceName.Should().Be("FlightService");
        route.TargetBaseUrl.Should().Be("http://flightservice:5002");
    }
}