using Dispatcher.Application.Forwarding;
using Dispatcher.Domain.Routing;
using Microsoft.AspNetCore.Mvc;
using Dispatcher.Infrastructure.Routing;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api")]
public class GatewayController : ControllerBase
{
    private readonly IRequestForwarder _requestForwarder;
    private readonly IRouteResolver _routeResolver;

    public GatewayController(IRequestForwarder requestForwarder, IRouteResolver routeResolver)
    {
        _requestForwarder = requestForwarder;
        _routeResolver = routeResolver;
    }

    [HttpGet("flights")]
    public async Task<IActionResult> GetFlights()
    {
        var route = _routeResolver.Resolve("/api/flights", "GET");

        if (route is null)
            return NotFound();

        var result = await _requestForwarder.ForwardAsync(route.TargetBaseUrl);
        return Ok(result);
    }

    [HttpPost("flights")]
    public IActionResult PostFlights()
    {
        return Ok("Forwarded to FlightService");
    }
}