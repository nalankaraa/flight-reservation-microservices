using Dispatcher.Domain.Routing;

namespace Dispatcher.Application.Routing;

public interface IRouteRepository
{
    Task<RouteDefinition?> FindRouteAsync(string path, string method);
    Task AddRouteAsync(RouteDefinition route);
    Task UpsertRouteAsync(RouteDefinition route);
    Task<List<RouteDefinition>> GetAllRoutesAsync();
}