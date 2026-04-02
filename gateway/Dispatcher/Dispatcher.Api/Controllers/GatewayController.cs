using Dispatcher.Application.Forwarding;
using Dispatcher.Domain.Routing;
using Microsoft.AspNetCore.Mvc;
using Dispatcher.Infrastructure.Routing;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api/{**path}")]
public class GatewayController : ControllerBase
{
    private readonly IRequestForwarder _requestForwarder;
    private readonly IRouteResolver _routeResolver;

    public GatewayController(IRequestForwarder requestForwarder, IRouteResolver routeResolver)
    {
        _requestForwarder = requestForwarder;
        _routeResolver = routeResolver;
    }

    [AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH")]
    public async Task<IActionResult> Forward(string? path)
    {
<<<<<<< Updated upstream
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
=======
        var requestPath = "/api";

        if (!string.IsNullOrWhiteSpace(path))
            requestPath += "/" + path.TrimStart('/');

        return await ForwardRequest(requestPath, Request.Method);
    }

    private async Task<IActionResult> ForwardRequest(string path, string method)
    {
        var route = await _routeResolver.ResolveAsync(path, method);
>>>>>>> Stashed changes

        if (route is null)
            return NotFound();

        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        var queryString = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
        var targetUrl = route.TargetBaseUrl.TrimEnd('/') + path + queryString;

<<<<<<< Updated upstream
        var result = await _requestForwarder.ForwardAsync(
            route.TargetBaseUrl,
            Request.Method,
            headers,
            Request.Body
        );

        return Ok(result);
=======
        try
        {
            var response = await _requestForwarder.ForwardAsync(
                method,
                targetUrl,
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
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, "Bad Gateway");
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service Unavailable");
        }
>>>>>>> Stashed changes
    }
}
