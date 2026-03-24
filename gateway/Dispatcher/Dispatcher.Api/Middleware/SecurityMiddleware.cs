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

        // AUTH CHECK (401)
        if (IsProtectedRoute(path) && !HasAuthorizationHeader(context))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        // ROLE CHECK (403)
        if (IsAdminRoute(path, method) && !IsAdmin(context))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden");
            return;
        }

        // Forwarding YOK  Controller yapacak
        await _next(context);
    }

    // ============================
    // HELPER METHODS
    // ============================

    private static bool HasAuthorizationHeader(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(authHeader);
    }

    private static bool IsAdmin(HttpContext context)
    {
        var roleHeader = context.Request.Headers["Role"].FirstOrDefault();
        return string.Equals(roleHeader, "Admin", StringComparison.OrdinalIgnoreCase);
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