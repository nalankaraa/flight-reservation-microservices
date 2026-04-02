using Dispatcher.Application.Forwarding;
using Dispatcher.Domain.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api/{**catchAll}")]
public class GatewayController : ControllerBase
{
    private readonly IRequestForwarder _requestForwarder;
    private readonly IRouteResolver _routeResolver;

    public GatewayController(IRequestForwarder requestForwarder, IRouteResolver routeResolver)
    {
        _requestForwarder = requestForwarder;
        _routeResolver = routeResolver;
    }

    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE")]
    public async Task<IActionResult> Proxy(string catchAll)
    {
        return await ForwardRequest($"/api/{catchAll}", Request.Method);
    }

    private async Task<IActionResult> ForwardRequest(string path, string method)
    {
        var route = await _routeResolver.ResolveAsync(path, method);

        if (route is null)
            return NotFound();

        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        var queryString = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;

        try
        {
            var response = await _requestForwarder.ForwardAsync(
                method,
                route.TargetBaseUrl.TrimEnd('/') + path + queryString,
                headers,
                Request.Body
            );

            var content = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                ContentType = "application/json"
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
    }
}
