using Dispatcher.Domain.Routing;

namespace Dispatcher.Api.Middleware;

public class SecurityMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRouteResolver routeResolver)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        var route = await routeResolver.ResolveAsync(path, method);

        if (route is not null)
        {
            if (route.RequiresAuth && !HasAuthorizationHeader(context))
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
        }

        await _next(context);
    }

    private static bool HasAuthorizationHeader(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(authHeader);
    }

    private static bool IsUserInAllowedRoles(HttpContext context, List<string> allowedRoles)
    {
        var roleHeader = context.Request.Headers["Role"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(roleHeader))
            return false;

        return allowedRoles.Any(role =>
            string.Equals(role, roleHeader, StringComparison.OrdinalIgnoreCase));
    }
}