using System.Diagnostics;
using Dispatcher.Application.Logging;
using Dispatcher.Api.Observability;
using Dispatcher.Domain.Logging;
using Dispatcher.Domain.Routing;
using System.Security.Claims;

namespace Dispatcher.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IRequestLogRepository logRepository,
        IRouteResolver routeResolver,
        DispatcherMetricsStore metricsStore)
    {
        var stopwatch = Stopwatch.StartNew();
        var route = context.Items.TryGetValue(DispatcherRequestLogContextKeys.ResolvedRoute, out var resolvedRoute)
            ? resolvedRoute as RouteDefinition
            : await routeResolver.ResolveAsync(
                context.Request.Path.Value ?? string.Empty,
                context.Request.Method);

        if (route is not null)
        {
            context.Items[DispatcherRequestLogContextKeys.ResolvedRoute] = route;
            context.Items[DispatcherRequestLogContextKeys.TargetService] = route.TargetServiceName;
        }

        await _next(context);

        stopwatch.Stop();

        var log = new RequestLog
        {
            Path = context.Request.Path.Value ?? string.Empty,
            Method = context.Request.Method,
            StatusCode = context.Response.StatusCode,
            DurationMs = stopwatch.Elapsed.TotalMilliseconds,
            TimestampUtc = DateTime.UtcNow,
            UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
            UserRole = string.Join(",", context.User.FindAll(ClaimTypes.Role).Select(x => x.Value)),
            TargetService = context.Items[DispatcherRequestLogContextKeys.TargetService] as string,
            ErrorMessage = ResolveErrorMessage(context)
        };

        metricsStore.RecordRequest(
            log.Path,
            log.Method,
            log.StatusCode,
            log.DurationMs,
            log.TargetService);

        _ = logRepository.AddAsync(log);
    }

    private static string? ResolveErrorMessage(HttpContext context)
    {
        if (context.Items.TryGetValue(DispatcherRequestLogContextKeys.ErrorMessage, out var errorMessage)
            && errorMessage is string message
            && !string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return context.Response.StatusCode switch
        {
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status502BadGateway => "Bad Gateway",
            StatusCodes.Status503ServiceUnavailable => "Service Unavailable",
            >= 400 => $"HTTP {context.Response.StatusCode}",
            _ => null
        };
    }
}
