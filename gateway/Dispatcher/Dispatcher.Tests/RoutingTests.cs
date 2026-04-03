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
        route.TargetBaseUrl.Should().Be("http://localhost:5162");
    }

    [Fact]
    public async Task ResolveAsync_Should_Return_PaymentService_Route_For_PostPayments()
    {
        var resolver = new InMemoryRouteResolver();

        var route = await resolver.ResolveAsync("/api/payments/payment-1/complete", "POST");

        route.Should().NotBeNull();
        route!.PathPrefix.Should().Be("/api/payments");
        route.HttpMethod.Should().Be("POST");
        route.TargetServiceName.Should().Be("PaymentService");
        route.TargetBaseUrl.Should().Be("http://localhost:5110");
    }

    [Fact]
    public async Task ResolveAsync_Should_Return_NotificationService_Route_For_GetNotifications()
    {
        var resolver = new InMemoryRouteResolver();

        var route = await resolver.ResolveAsync("/api/notifications/user/user-1", "GET");

        route.Should().NotBeNull();
        route!.PathPrefix.Should().Be("/api/notifications");
        route.HttpMethod.Should().Be("GET");
        route.TargetServiceName.Should().Be("NotificationService");
        route.TargetBaseUrl.Should().Be("http://localhost:5270");
    }
}
