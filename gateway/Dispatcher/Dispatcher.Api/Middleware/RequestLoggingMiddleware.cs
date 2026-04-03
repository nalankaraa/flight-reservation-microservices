using System.Diagnostics;
using Dispatcher.Application.Logging;
using Dispatcher.Domain.Logging;
using Dispatcher.Domain.Routing;

namespace Dispatcher.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRequestLogRepository logRepository, IRouteResolver routeResolver)
    {
        var stopwatch = Stopwatch.StartNew();
        var route = await routeResolver.ResolveAsync(
            context.Request.Path.Value ?? string.Empty,
            context.Request.Method);

        if (route is not null)
        {
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
            TargetService = context.Items[DispatcherRequestLogContextKeys.TargetService] as string,
            ErrorMessage = ResolveErrorMessage(context)
        };

        await logRepository.AddAsync(log);
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