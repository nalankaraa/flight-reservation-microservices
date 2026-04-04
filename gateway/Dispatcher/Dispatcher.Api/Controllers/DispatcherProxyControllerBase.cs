using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Dispatcher.Application.Forwarding;
using Dispatcher.Api.Middleware;
using Dispatcher.Domain.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Dispatcher.Api.Controllers;

public abstract class DispatcherProxyControllerBase : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IRequestForwarder _requestForwarder;
    private readonly IRouteResolver _routeResolver;

    protected DispatcherProxyControllerBase(IRequestForwarder requestForwarder, IRouteResolver routeResolver)
    {
        _requestForwarder = requestForwarder;
        _routeResolver = routeResolver;
    }

    protected Task<IActionResult> ForwardAsync(string path, string method)
    {
        return ForwardCoreAsync(path, method, null);
    }

    protected Task<IActionResult> ForwardAsync<TRequest>(string path, string method, TRequest request)
    {
        return ForwardCoreAsync(path, method, request);
    }

    private async Task<IActionResult> ForwardCoreAsync(string path, string method, object? requestBody)
    {
        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        headers.Remove("Content-Length");

        Stream bodyStream = Stream.Null;

        if (requestBody is not null)
        {
            headers["Content-Type"] = MediaTypeNames.Application.Json;
            bodyStream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(requestBody, JsonOptions));
        }

        var route = HttpContext.Items.TryGetValue(DispatcherRequestLogContextKeys.ResolvedRoute, out var resolvedRoute)
            ? resolvedRoute as RouteDefinition
            : await _routeResolver.ResolveAsync(path, method);

        if (route is null)
        {
            HttpContext.Items[DispatcherRequestLogContextKeys.ErrorMessage] = "Not Found";
            return NotFound();
        }

        HttpContext.Items[DispatcherRequestLogContextKeys.ResolvedRoute] = route;
        HttpContext.Items[DispatcherRequestLogContextKeys.TargetService] = route.TargetServiceName;

        var targetUrl = route.TargetBaseUrl.TrimEnd('/') + path + (Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty);

        try
        {
            using var response = await _requestForwarder.ForwardAsync(method, targetUrl, headers, bodyStream);
            var content = await response.Content.ReadAsStringAsync();

            var contentType = response.Content.Headers.ContentType?.ToString() ?? MediaTypeNames.Application.Json;

            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                ContentType = contentType
            };
        }
        catch (HttpRequestException)
        {
            HttpContext.Items[DispatcherRequestLogContextKeys.ErrorMessage] = "Bad Gateway";
            return StatusCode(StatusCodes.Status502BadGateway, "Bad Gateway");
        }
        catch (TaskCanceledException)
        {
            HttpContext.Items[DispatcherRequestLogContextKeys.ErrorMessage] = "Service Unavailable";
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service Unavailable");
        }
    }
}
