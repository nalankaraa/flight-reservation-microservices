using AvailabilityService.Application.Clients;

namespace AvailabilityService.Tests;

public class FakeFlightCapacityClient : IFlightCapacityClient
{
    public bool ShouldFail { get; set; }
    public int AvailableSeatCount { get; set; } = 10;

    public Task<FlightCapacityResult> GetCapacityAsync(string flightId, string authorizationHeader)
    {
        if (ShouldFail)
        {
            return Task.FromResult(new FlightCapacityResult());
        }

        return Task.FromResult(new FlightCapacityResult
        {
            Success = true,
            AvailableSeatCount = AvailableSeatCount
        });
    }
}
