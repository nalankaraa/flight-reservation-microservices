namespace AvailabilityService.Application.Clients;

public interface IFlightCapacityClient
{
    Task<FlightCapacityResult> GetCapacityAsync(string flightId, string authorizationHeader);
}
