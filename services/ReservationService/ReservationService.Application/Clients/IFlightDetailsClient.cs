namespace ReservationService.Application.Clients;

public interface IFlightDetailsClient
{
    Task<FlightLookupResult> GetByIdAsync(string flightId, string authorizationHeader);
}
