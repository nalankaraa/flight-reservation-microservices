using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Dispatcher.Tests;

public class ErrorHandlingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ErrorHandlingTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnknownRoute_Should_Return404()
    {
        // Act
        var response = await _client.GetAsync("/api/unknown");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
<<<<<<< Updated upstream
}
=======

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
        request.Headers.Add("Authorization", $"Bearer {JwtTestTokenFactory.CreateToken("admin-1", "admin@test.com", "Admin")}");

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
        request.Headers.Add("Authorization", $"Bearer {JwtTestTokenFactory.CreateToken("admin-1", "admin@test.com", "Admin")}");

        request.Content = new StringContent(
            "{\"from\":\"IST\",\"to\":\"ANK\"}",
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task GetFlights_Should_Return503_When_Downstream_Service_Times_Out()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IRequestForwarder, TimeoutRequestForwarder>();
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/flights");
        request.Headers.Add("Authorization", $"Bearer {JwtTestTokenFactory.CreateToken("admin-1", "admin@test.com", "Admin")}");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
}
>>>>>>> Stashed changes
