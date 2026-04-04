using Dispatcher.Domain.Routing;
using System.Security.Claims;

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
            context.Items[DispatcherRequestLogContextKeys.ResolvedRoute] = route;
            context.Items[DispatcherRequestLogContextKeys.TargetService] = route.TargetServiceName;

            if (route.RequiresAuth && !IsAuthenticated(context))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            if (route.RequiresAuth && route.AllowedRoles.Any() && !IsUserInAllowedRoles(context, route.AllowedRoles))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden");
                return;
            }
        }

        await _next(context);
    }

    private static bool IsAuthenticated(HttpContext context)
    {
        return context.User.Identity?.IsAuthenticated == true;
    }

    private static bool IsUserInAllowedRoles(HttpContext context, List<string> allowedRoles)
    {
        var userRoles = context.User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return allowedRoles.Any(userRoles.Contains);
    }
}
