using Dispatcher.Domain.Routing;

namespace Dispatcher.Infrastructure.Routing;

public class InMemoryRouteResolver : IRouteResolver
{
    //  List yerine IReadOnlyList (immutability)
    private readonly IReadOnlyList<RouteDefinition> _routes;

    public InMemoryRouteResolver()
    {
        _routes = new List<RouteDefinition>
        {
            new RouteDefinition
            {
                PathPrefix = "/api/flights",
                HttpMethod = "GET",
                TargetServiceName = "FlightService",
                TargetBaseUrl = "http://flightservice:5002"
            }
        };
    }

    public RouteDefinition? Resolve(string path, string method)
    {
        // Guard clause (null/empty check)
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(method))
            return null;

        return _routes.FirstOrDefault(route =>
            path.StartsWith(route.PathPrefix, StringComparison.OrdinalIgnoreCase) &&
            route.HttpMethod.Equals(method, StringComparison.OrdinalIgnoreCase));
    }
}