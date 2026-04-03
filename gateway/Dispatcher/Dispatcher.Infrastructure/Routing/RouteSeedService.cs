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
        var authBaseUrl = _configuration["Services:Auth:BaseUrl"] ?? "http://localhost:5078";
        var flightBaseUrl = _configuration["Services:Flight:BaseUrl"] ?? "http://localhost:5162";
        var reservationBaseUrl = _configuration["Services:Reservation:BaseUrl"] ?? "http://localhost:5029";
        var availabilityBaseUrl = _configuration["Services:Availability:BaseUrl"] ?? "http://localhost:5099";
        var paymentBaseUrl = _configuration["Services:Payment:BaseUrl"] ?? "http://localhost:5110";
        var notificationBaseUrl = _configuration["Services:Notification:BaseUrl"] ?? "http://localhost:5270";

        await AddRouteAsync("/api/auth/register", "POST", "AuthService", authBaseUrl, false);
        await AddRouteAsync("/api/auth/login", "POST", "AuthService", authBaseUrl, false);
        await AddRouteAsync("/api/auth/me", "GET", "AuthService", authBaseUrl, true, "Admin", "Customer");

        await AddRouteAsync("/api/flights", "GET", "FlightService", flightBaseUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/flights", "POST", "FlightService", flightBaseUrl, true, "Admin");
        await AddRouteAsync("/api/flights", "PUT", "FlightService", flightBaseUrl, true, "Admin");
        await AddRouteAsync("/api/flights", "DELETE", "FlightService", flightBaseUrl, true, "Admin");

        await AddRouteAsync("/api/reservations/my", "GET", "ReservationService", reservationBaseUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/reservations", "GET", "ReservationService", reservationBaseUrl, true, "Admin");
        await AddRouteAsync("/api/reservations", "POST", "ReservationService", reservationBaseUrl, true, "Admin", "Customer");

        await AddRouteAsync("/api/availability", "GET", "AvailabilityService", availabilityBaseUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/availability", "POST", "AvailabilityService", availabilityBaseUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/payments", "GET", "PaymentService", paymentBaseUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/payments", "POST", "PaymentService", paymentBaseUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/notifications", "GET", "NotificationService", notificationBaseUrl, true, "Admin", "Customer");
        await AddRouteAsync("/api/notifications", "POST", "NotificationService", notificationBaseUrl, true, "Admin", "Customer");
    }

    private Task AddRouteAsync(
        string pathPrefix,
        string httpMethod,
        string serviceName,
        string targetBaseUrl,
        bool requiresAuth,
        params string[] allowedRoles)
    {
        return _routeRepository.UpsertRouteAsync(new RouteDefinition
        {
            Id = Guid.NewGuid().ToString(),
            PathPrefix = pathPrefix,
            HttpMethod = httpMethod,
            TargetServiceName = serviceName,
            TargetBaseUrl = targetBaseUrl,
            RequiresAuth = requiresAuth,
            AllowedRoles = allowedRoles.ToList()
        });
    }
}
