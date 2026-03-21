using Dispatcher.Domain.Routing;

namespace Dispatcher.Infrastructure.Routing;

public class InMemoryRouteResolver : IRouteResolver
{
    private readonly List<RouteDefinition> _routes = new();

    public RouteDefinition? Resolve(string path, string method)
    {
        return _routes.FirstOrDefault(route =>
            path.StartsWith(route.PathPrefix, StringComparison.OrdinalIgnoreCase) &&
            route.HttpMethod.Equals(method, StringComparison.OrdinalIgnoreCase));
    }
}