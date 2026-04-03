using System.Net;
using FluentAssertions;
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
    public async Task GetFlights_Should_Return401_When_Token_Is_Expired()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/flights");
        request.Headers.Add("Authorization", $"Bearer {JwtTestTokenFactory.CreateToken("Customer", DateTime.UtcNow.AddSeconds(-30))}");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}