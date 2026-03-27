using Dispatcher.Application.Forwarding;
using Dispatcher.Domain.Routing;
using Microsoft.AspNetCore.Mvc;

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
        return await ForwardRequest("/api/flights", "GET");
    }

    [HttpPost("flights")]
    public async Task<IActionResult> PostFlights()
    {
        return await ForwardRequest("/api/flights", "POST");
    }

    private async Task<IActionResult> ForwardRequest(string path, string method)
    {
        var route = _routeResolver.Resolve(path, method);

        if (route is null)
            return NotFound();

        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

        var response = await _requestForwarder.ForwardAsync(
            method,
            route.TargetBaseUrl + path,
            headers,
            Request.Body
        );

        var content = await response.Content.ReadAsStringAsync();
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";

        return new ContentResult
        {
            StatusCode = (int)response.StatusCode,
            Content = content,
            ContentType = contentType
        };
    }
}