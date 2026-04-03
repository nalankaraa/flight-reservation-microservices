using ReservationService.Application.Clients;

namespace ReservationService.Tests;

public class FakeFlightDetailsClient : IFlightDetailsClient
{
    public bool ShouldReturnNotFound { get; set; }
    public bool ShouldBeUnavailable { get; set; }
    public decimal Price { get; set; } = 2500m;

    public Task<FlightLookupResult> GetByIdAsync(string flightId, string authorizationHeader)
    {
        if (ShouldReturnNotFound)
        {
            return Task.FromResult(new FlightLookupResult
            {
                IsNotFound = true
            });
        }

        if (ShouldBeUnavailable)
        {
            return Task.FromResult(new FlightLookupResult
            {
                IsServiceUnavailable = true
            });
        }

        return Task.FromResult(new FlightLookupResult
        {
            Success = true,
            Price = Price
        });
    }
}
