using Dispatcher.Application.Routing;
using Dispatcher.Domain.Routing;

namespace Dispatcher.Infrastructure.Routing;

public class RouteSeedService
{
    private readonly IRouteRepository _routeRepository;

    public RouteSeedService(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task SeedAsync()
    {
        var existingRoutes = await _routeRepository.GetAllRoutesAsync();

        if (existingRoutes.Any())
            return;

        await _routeRepository.AddRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = "/api/flights",
            HttpMethod = "GET",
            TargetServiceName = "FlightService",
            TargetBaseUrl = "http://flightservice:5002",
            RequiresAuth = true,
            AllowedRoles = new List<string> { "Admin", "User" }
        });

        await _routeRepository.AddRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = "/api/flights",
            HttpMethod = "POST",
            TargetServiceName = "FlightService",
            TargetBaseUrl = "http://flightservice:5002",
            RequiresAuth = true,
            AllowedRoles = new List<string> { "Admin" }
        });

        await _routeRepository.AddRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = "/api/notifications",
            HttpMethod = "GET",
            TargetServiceName = "NotificationService",
            TargetBaseUrl = "http://localhost:5179",
            RequiresAuth = true,
            AllowedRoles = new List<string> { "Admin", "User" }
        });

        await _routeRepository.AddRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = "/api/notifications",
            HttpMethod = "POST",
            TargetServiceName = "NotificationService",
            TargetBaseUrl = "http://localhost:5179",
            RequiresAuth = true,
            AllowedRoles = new List<string> { "Admin", "User" }
        });
    }
}