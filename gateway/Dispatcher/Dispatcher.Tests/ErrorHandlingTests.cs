using System.Net;
using Dispatcher.Application.Forwarding;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dispatcher.Tests;

public class ErrorHandlingTests : IClassFixture<DispatcherWebApplicationFactory>
{
    private readonly DispatcherWebApplicationFactory _factory;

    public ErrorHandlingTests(DispatcherWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UnknownRoute_Should_Return404()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/unknown");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFlights_Should_Return502_When_Downstream_Service_Fails()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IRequestForwarder, ThrowingRequestForwarder>();
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/flights");
        request.Headers.Add("Authorization", "Bearer fake-token");
        request.Headers.Add("Role", "Admin");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }
    [Fact]
    public async Task PostFlights_Should_Return502_When_Downstream_Service_Fails()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IRequestForwarder, ThrowingRequestForwarder>();
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/flights");
        request.Headers.Add("Authorization", "Bearer fake-token");
        request.Headers.Add("Role", "Admin");

        request.Content = new StringContent(
            "{\"from\":\"IST\",\"to\":\"ANK\"}",
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }
}
