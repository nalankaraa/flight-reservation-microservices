namespace Dispatcher.Domain.Routing;

public interface IRouteResolver
{
    Task<RouteDefinition?> ResolveAsync(string path, string method);
}