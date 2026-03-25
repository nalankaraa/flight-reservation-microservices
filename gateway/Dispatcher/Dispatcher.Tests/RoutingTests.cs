using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Routing;
using FluentAssertions;
using Xunit;

namespace Dispatcher.Tests;

public class RoutingTests
{
    [Fact]
    public void Resolve_Should_Return_FlightService_Route_For_GetFlights()
    {
        // Arrange
        var resolver = new InMemoryRouteResolver();

        // Act
        var route = resolver.Resolve("/api/flights", "GET");

        // Assert
        route.Should().NotBeNull();
        route!.PathPrefix.Should().Be("/api/flights");
        route.HttpMethod.Should().Be("GET");
        route.TargetServiceName.Should().Be("FlightService");
        route.TargetBaseUrl.Should().Be("http://flightservice:5002");
    }
}