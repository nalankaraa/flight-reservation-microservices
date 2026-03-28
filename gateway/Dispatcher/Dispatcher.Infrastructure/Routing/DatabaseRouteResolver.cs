using Dispatcher.Application.Routing;
using Dispatcher.Domain.Routing;

namespace Dispatcher.Infrastructure.Routing;

public class DatabaseRouteResolver : IRouteResolver
{
    private readonly IRouteRepository _routeRepository;

    public DatabaseRouteResolver(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public RouteDefinition? Resolve(string path, string method)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(method))
            return null;

        return _routeRepository.FindRouteAsync(path, method).GetAwaiter().GetResult();
    }

    public Task<RouteDefinition?> ResolveAsync(string path, string method)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(method))
            return Task.FromResult<RouteDefinition?>(null);

        return _routeRepository.FindRouteAsync(path, method);
    }
}