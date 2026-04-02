<<<<<<< Updated upstream
=======
using Dispatcher.Domain.Routing;
using System.Security.Claims;

>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
=======
            if (route.RequiresAuth && !IsAuthenticated(context))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            if (route.AllowedRoles.Any() && !IsUserInAllowedRoles(context, route.AllowedRoles))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden");
                return;
            }
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
    // ============================
    // HELPER METHODS
    // ============================

    private static bool HasAuthorizationHeader(HttpContext context)
=======
    private static bool IsAuthenticated(HttpContext context)
>>>>>>> Stashed changes
    {
        return context.User.Identity?.IsAuthenticated == true;
    }

    private static bool IsAdmin(HttpContext context)
    {
<<<<<<< Updated upstream
        var roleHeader = context.Request.Headers["Role"].FirstOrDefault();
        return string.Equals(roleHeader, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProtectedRoute(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return path.StartsWith("/api/flights");
=======
        var roles = context.User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return roles.Count > 0 && allowedRoles.Any(roles.Contains);
>>>>>>> Stashed changes
    }

    private static bool IsAdminRoute(string? path, string method)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return path.StartsWith("/api/flights") &&
               method.Equals("POST", StringComparison.OrdinalIgnoreCase);
    }
}