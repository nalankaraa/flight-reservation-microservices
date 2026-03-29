using Dispatcher.Application.Routing;
using Dispatcher.Domain.Routing;

namespace Dispatcher.Tests;

public class InMemoryRouteRepository : IRouteRepository
{
    private readonly List<RouteDefinition> _routes = new();

    public Task<RouteDefinition?> FindRouteAsync(string path, string method)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(method))
            return Task.FromResult<RouteDefinition?>(null);

        var route = _routes.FirstOrDefault(route =>
            path.StartsWith(route.PathPrefix, StringComparison.OrdinalIgnoreCase) &&
            route.HttpMethod.Equals(method, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(route);
    }

    public Task AddRouteAsync(RouteDefinition route)
    {
        _routes.Add(route);
        return Task.CompletedTask;
    }

    public Task<List<RouteDefinition>> GetAllRoutesAsync()
    {
        return Task.FromResult(_routes.ToList());
    }
}
