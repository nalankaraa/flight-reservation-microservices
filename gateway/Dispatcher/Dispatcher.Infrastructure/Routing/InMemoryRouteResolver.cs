using Dispatcher.Domain.Routing;

namespace Dispatcher.Infrastructure.Routing;

public class InMemoryRouteResolver : IRouteResolver
{
    private readonly IReadOnlyList<RouteDefinition> _routes;

    public InMemoryRouteResolver()
    {
        _routes = new List<RouteDefinition>
        {
            new RouteDefinition
            {
                Id = Guid.NewGuid().ToString(),
                PathPrefix = "/api/flights",
                HttpMethod = "GET",
                TargetServiceName = "FlightService",
                TargetBaseUrl = "http://localhost:5162",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "Admin", "Customer" }
            },
            new RouteDefinition
            {
                Id = Guid.NewGuid().ToString(),
                PathPrefix = "/api/flights",
                HttpMethod = "POST",
                TargetServiceName = "FlightService",
                TargetBaseUrl = "http://localhost:5162",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "Admin" }
            },
            new RouteDefinition
            {
                Id = Guid.NewGuid().ToString(),
                PathPrefix = "/api/payments",
                HttpMethod = "GET",
                TargetServiceName = "PaymentService",
                TargetBaseUrl = "http://localhost:5110",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "Admin", "Customer" }
            },
            new RouteDefinition
            {
                Id = Guid.NewGuid().ToString(),
                PathPrefix = "/api/payments",
                HttpMethod = "POST",
                TargetServiceName = "PaymentService",
                TargetBaseUrl = "http://localhost:5110",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "Admin", "Customer" }
            },
            new RouteDefinition
            {
                Id = Guid.NewGuid().ToString(),
                PathPrefix = "/api/notifications",
                HttpMethod = "GET",
                TargetServiceName = "NotificationService",
                TargetBaseUrl = "http://localhost:5270",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "Admin", "Customer" }
            },
            new RouteDefinition
            {
                Id = Guid.NewGuid().ToString(),
                PathPrefix = "/api/notifications",
                HttpMethod = "POST",
                TargetServiceName = "NotificationService",
                TargetBaseUrl = "http://localhost:5270",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "Admin", "Customer" }
            }
        };
    }

    public Task<RouteDefinition?> ResolveAsync(string path, string method)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(method))
            return Task.FromResult<RouteDefinition?>(null);

        var route = _routes.FirstOrDefault(route =>
            path.StartsWith(route.PathPrefix, StringComparison.OrdinalIgnoreCase) &&
            route.HttpMethod.Equals(method, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult<RouteDefinition?>(route);
    }
}
