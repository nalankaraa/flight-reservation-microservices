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
}