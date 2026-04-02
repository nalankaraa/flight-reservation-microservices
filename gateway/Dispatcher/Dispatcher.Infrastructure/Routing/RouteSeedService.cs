using Dispatcher.Application.Routing;
using Dispatcher.Domain.Routing;
using Microsoft.Extensions.Configuration;

namespace Dispatcher.Infrastructure.Routing;

public class RouteSeedService
{
    private readonly IRouteRepository _routeRepository;
    private readonly IConfiguration _configuration;

    public RouteSeedService(IRouteRepository routeRepository, IConfiguration configuration)
    {
        _routeRepository = routeRepository;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var authServiceUrl = GetServiceBaseUrl("Auth", "http://localhost:5078");
        var availabilityServiceUrl = GetServiceBaseUrl("Availability", "http://localhost:5099");
        var flightServiceUrl = GetServiceBaseUrl("Flight", "http://localhost:5162");
        var notificationServiceUrl = GetServiceBaseUrl("Notification", "http://localhost:5179");
        var paymentServiceUrl = GetServiceBaseUrl("Payment", "http://localhost:5027");
        var reservationServiceUrl = GetServiceBaseUrl("Reservation", "http://localhost:5029");

        await AddRouteAsync("/api/auth/register", "POST", "AuthService", authServiceUrl, false);
        await AddRouteAsync("/api/auth/login", "POST", "AuthService", authServiceUrl, false);
        await AddRouteAsync("/api/auth/me", "GET", "AuthService", authServiceUrl, true, "Admin", "Customer");

        await AddRouteAsync("/api/availability", "GET", "AvailabilityService", availabilityServiceUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/availability", "POST", "AvailabilityService", availabilityServiceUrl, true, "Admin", "Customer");

        await AddRouteAsync("/api/flights", "GET", "FlightService", flightServiceUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/flights", "POST", "FlightService", flightServiceUrl, true, "Admin");
        await AddRouteAsync("/api/flights", "PUT", "FlightService", flightServiceUrl, true, "Admin");
        await AddRouteAsync("/api/flights", "DELETE", "FlightService", flightServiceUrl, true, "Admin");

        await AddRouteAsync("/api/notifications", "GET", "NotificationService", notificationServiceUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/notifications", "POST", "NotificationService", notificationServiceUrl, true, "Admin", "Customer");

        await AddRouteAsync("/api/payments", "GET", "PaymentService", paymentServiceUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/payments", "POST", "PaymentService", paymentServiceUrl, true, "Admin", "Customer");

        await AddRouteAsync("/api/reservations/my", "GET", "ReservationService", reservationServiceUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/reservations", "GET", "ReservationService", reservationServiceUrl, true, "Admin");
        await AddRouteAsync("/api/reservations", "POST", "ReservationService", reservationServiceUrl, true, "Admin", "Customer");
    }

    private Task AddRouteAsync(
        string pathPrefix,
        string method,
        string serviceName,
        string targetBaseUrl,
        bool requiresAuth = true,
        params string[] allowedRoles)
    {
        return _routeRepository.UpsertRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = pathPrefix,
            HttpMethod = method,
            TargetServiceName = serviceName,
            TargetBaseUrl = targetBaseUrl,
            RequiresAuth = requiresAuth,
            AllowedRoles = allowedRoles.ToList()
        });
    }

    private string GetServiceBaseUrl(string serviceName, string defaultUrl)
    {
        return _configuration[$"Services:{serviceName}:BaseUrl"] ?? defaultUrl;
    }
}
