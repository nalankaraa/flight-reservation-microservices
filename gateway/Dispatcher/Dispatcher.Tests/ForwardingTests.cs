using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Dispatcher.Application.Forwarding;

namespace Dispatcher.Tests;

public class ForwardingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ForwardingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetFlights_Should_Return_Forwarded_Response_From_FlightService()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IRequestForwarder, FakeRequestForwarder>();
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/flights");
        request.Headers.Add("Authorization", "Bearer fake-token");

        // Act
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().Be("Flights forwarded successfully");
    }
}