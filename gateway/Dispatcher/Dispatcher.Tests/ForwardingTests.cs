using Dispatcher.Application.Forwarding;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using Xunit;

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
        request.Headers.Add("Role", "User");

        // Act
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().Be("Flights forwarded successfully");
    }

    [Fact]
    public async Task PostFlights_Should_Return_Forwarded_Response_When_User_Is_Admin()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IRequestForwarder, FakeRequestForwarder>();
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/flights");
        request.Headers.Add("Authorization", "Bearer fake-token");
        request.Headers.Add("Role", "Admin");
        request.Content = new StringContent(
            "{\"from\":\"IST\",\"to\":\"ANK\"}",
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("Flights forwarded successfully");
    }
}