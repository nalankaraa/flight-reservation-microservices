namespace Dispatcher.Domain.Routing;
public class RouteDefinition
{
    public string Id { get; set; } = default!;
    public string PathPrefix { get; set; } = default!;
    public string HttpMethod { get; set; } = default!;
    public string TargetServiceName { get; set; } = default!;
    public string TargetBaseUrl { get; set; } = default!;
    public bool RequiresAuth { get; set; }
    public List<string> AllowedRoles { get; set; } = new();
}