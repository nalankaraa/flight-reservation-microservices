using Dispatcher.Application.Forwarding;
using Dispatcher.Application.Logging;
using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Routing;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IRequestForwarder>();
                services.RemoveAll<IRequestLogRepository>();
                services.RemoveAll<IRouteResolver>();
                services.AddSingleton<IRequestForwarder, FakeRequestForwarder>();
                services.AddSingleton<IRequestLogRepository>(fakeLogRepository);
                services.AddSingleton<IRouteResolver, InMemoryRouteResolver>();
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/flights");
        request.Headers.Add("Authorization", "Bearer fake-token");
        request.Headers.Add("Role", "User");

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
