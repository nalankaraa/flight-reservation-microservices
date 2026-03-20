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
        var method = context.Request.Method;

        //  401 check
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

        //  403 check (YENÝ EKLENEN)
        if (IsAdminRoute(path, method))
        {
            var roleHeader = context.Request.Headers["Role"].FirstOrDefault();

            if (!string.Equals(roleHeader, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden");
                return;
            }
        }

        await _next(context);
    }

    private static bool IsProtectedRoute(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return path.StartsWith("/api/flights");
    }

    private static bool IsAdminRoute(string? path, string method)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return path.StartsWith("/api/flights") &&
               method.Equals("POST", StringComparison.OrdinalIgnoreCase);
    }
}