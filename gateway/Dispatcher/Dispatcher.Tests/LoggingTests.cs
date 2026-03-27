using Dispatcher.Application.Forwarding;
using Dispatcher.Application.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dispatcher.Tests;

public class LoggingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LoggingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetFlights_Should_Create_A_Log_Record()
    {
        // Arrange
        var fakeLogRepository = new FakeRequestLogRepository();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IRequestForwarder, FakeRequestForwarder>();
                services.AddSingleton<IRequestLogRepository>(fakeLogRepository);
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/flights");
        request.Headers.Add("Authorization", "Bearer fake-token");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        fakeLogRepository.Logs.Should().NotBeEmpty();
        fakeLogRepository.Logs.Should().ContainSingle(log =>
            log.Path == "/api/flights" &&
            log.Method == "GET" &&
            log.StatusCode == 200 &&
            log.DurationMs >= 0 &&
            log.TimestampUtc <= DateTime.UtcNow);
    }
}