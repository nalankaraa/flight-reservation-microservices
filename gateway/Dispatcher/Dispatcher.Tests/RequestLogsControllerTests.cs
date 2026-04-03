using System.Security.Claims;
using Dispatcher.Api.Controllers;
using Dispatcher.Application.Logging;
using Dispatcher.Domain.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            TargetService = "FlightService"
        });

        var controller = CreateController(repository);

        var result = await controller.GetRecent(50);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeAssignableTo<List<RequestLog>>()
            .Which.Should().ContainSingle(log => log.Id == "log-1");
    }

    private static RequestLogsController CreateController(IRequestLogRepository repository)
    {
        return new RequestLogsController(repository)
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
}