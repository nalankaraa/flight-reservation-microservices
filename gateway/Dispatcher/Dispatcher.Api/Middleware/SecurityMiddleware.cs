using System.Net;

namespace Dispatcher.Api.Middleware;

public class SecurityMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();

        if (IsProtectedRoute(path))
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authHeader))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }

        await _next(context);
    }

    private static bool IsProtectedRoute(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var protectedRoutes = new[]
        {
            "/api/flights"
        };

        return protectedRoutes.Any(route => path.StartsWith(route));
    }
}