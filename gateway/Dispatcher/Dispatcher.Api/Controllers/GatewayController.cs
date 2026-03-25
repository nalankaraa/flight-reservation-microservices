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

        var response = await _requestForwarder.ForwardAsync(
            "GET",
            route.TargetBaseUrl + "/api/flights",
            new Dictionary<string, string>(),
            Stream.Null
        );

        var content = await response.Content.ReadAsStringAsync();

        return Content(content, "text/plain"); 
    }

    [HttpPost("flights")]
    public async Task<IActionResult> PostFlights()
    {
        var route = _routeResolver.Resolve("/api/flights", "POST");

        if (route is null)
            return NotFound();

        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

        var result = await _requestForwarder.ForwardAsync(
            route.TargetBaseUrl,
            Request.Method,
            headers,
            Request.Body
        );

        return Ok(result);
    }
}