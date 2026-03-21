namespace Dispatcher.Domain.Routing;

public interface IRouteResolver
{
    RouteDefinition? Resolve(string path, string method);
}