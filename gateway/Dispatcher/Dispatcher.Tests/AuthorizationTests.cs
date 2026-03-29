using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Dispatcher.Tests;

public class AuthorizationTests : IClassFixture<DispatcherWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthorizationTests(DispatcherWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetFlights_Should_Return401_When_Token_Is_Missing()
    {
        // Act
        var response = await _client.GetAsync("/api/flights");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostFlights_Should_Return403_When_User_Is_Not_Admin()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/flights");
        request.Headers.Add("Authorization", "Bearer fake-token");
        request.Headers.Add("Role", "User");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostFlights_Should_Not_Return403_When_User_Is_Admin()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/flights");
        request.Headers.Add("Authorization", "Bearer fake-token");
        request.Headers.Add("Role", "Admin");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetFlights_Should_Return401_When_AuthorizationHeader_Is_Empty()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/flights");
       

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFlights_Should_Not_Return403_For_User_Role()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/flights");
        request.Headers.Add("Authorization", "Bearer fake-token");
        request.Headers.Add("Role", "User");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
