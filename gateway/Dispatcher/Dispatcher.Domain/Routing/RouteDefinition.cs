namespace Dispatcher.Domain.Routing;

public class RouteDefinition
{
    public string PathPrefix { get; set; } = default!;
    public string HttpMethod { get; set; } = default!;
    public string TargetServiceName { get; set; } = default!;
    public string TargetBaseUrl { get; set; } = default!;
}