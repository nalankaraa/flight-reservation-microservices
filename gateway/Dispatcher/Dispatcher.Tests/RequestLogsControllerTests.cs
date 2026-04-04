using System.Security.Claims;
using Dispatcher.Api.Controllers;
using Dispatcher.Application.Logging;
using Dispatcher.Domain.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Dispatcher.Tests;

public class RequestLogsControllerTests
{
    [Fact]
    public async Task GetRecent_Should_Return_Logs_From_Repository()
    {
        var repository = new FakeRequestLogRepository();
        repository.Logs.Add(new RequestLog
        {
            Id = "log-1",
            TimestampUtc = DateTime.UtcNow,
            Path = "/api/flights",
            Method = "GET",
            StatusCode = 200,
            DurationMs = 10,
            UserId = "admin-1",
            UserRole = "Admin",
            TargetService = "FlightService"
        });

        var controller = CreateController(repository);

        var result = await controller.GetRecent(50);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeAssignableTo<List<RequestLog>>()
            .Which.Should().ContainSingle(log => log.Id == "log-1");
    }

    [Fact]
    public async Task Dashboard_Should_Render_Html_With_Log_Details()
    {
        var repository = new FakeRequestLogRepository();
        repository.Logs.Add(new RequestLog
        {
            Id = "log-1",
            TimestampUtc = new DateTime(2026, 04, 04, 10, 30, 00, DateTimeKind.Utc),
            Path = "/api/flights",
            Method = "GET",
            StatusCode = 502,
            DurationMs = 12.3,
            UserId = "user-1",
            UserRole = "Customer",
            TargetService = "FlightService",
            ErrorMessage = "Bad Gateway"
        });

        var controller = CreateController(repository);

        var result = await controller.Dashboard(50);

        result.Should().BeOfType<ContentResult>()
            .Which.ContentType.Should().Be("text/html; charset=utf-8");

        var content = ((ContentResult)result).Content;
        content.Should().Contain("Dispatcher Monitoring Console");
        content.Should().Contain("/api/flights");
        content.Should().Contain("FlightService");
        content.Should().Contain("Bad Gateway");
        content.Should().Contain("Overview");
        content.Should().Contain("Load Test Results");
        content.Should().Contain("Open Full Grafana");
        content.Should().Contain("panelId=1");
        content.Should().Contain("panelId=5");
        content.Should().Contain("user-1");
        content.Should().Contain("Customer");
        content.Should().Contain("Realistic-50");
        content.Should().Contain("Throughput");
        content.Should().Contain("Error / Conflict Rate");
    }

    [Fact]
    public async Task GetLoadTestResults_Should_Return_Realistic_Result_File_When_Present()
    {
        var controller = CreateController(new FakeRequestLogRepository());

        var result = await controller.GetLoadTestResults();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeAssignableTo<IEnumerable<object>>()
            .Which.Should().NotBeEmpty();
    }

    private static RequestLogsController CreateController(IRequestLogRepository repository)
    {
        return new RequestLogsController(repository, CreateEnvironment())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                        new Claim(ClaimTypes.Role, "Admin")
                    ], "TestAuth"))
                }
            }
        };
    }

    private static IHostEnvironment CreateEnvironment()
    {
        return new StubHostEnvironment
        {
            EnvironmentName = "Development",
            ApplicationName = "Dispatcher.Tests",
            ContentRootPath = "C:\\Users\\Esma Nur Mantı\\Desktop\\flight-reservation-microservices",
            ContentRootFileProvider = new NullFileProvider()
        };
    }

    private sealed class StubHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = default!;
        public string ApplicationName { get; set; } = default!;
        public string ContentRootPath { get; set; } = default!;
        public IFileProvider ContentRootFileProvider { get; set; } = default!;
    }
}
