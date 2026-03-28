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
                TargetBaseUrl = "http://flightservice:5002",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "Admin", "User" }
            },
            new RouteDefinition
            {
                Id = Guid.NewGuid().ToString(),
                PathPrefix = "/api/flights",
                HttpMethod = "POST",
                TargetServiceName = "FlightService",
                TargetBaseUrl = "http://flightservice:5002",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "Admin" }
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
