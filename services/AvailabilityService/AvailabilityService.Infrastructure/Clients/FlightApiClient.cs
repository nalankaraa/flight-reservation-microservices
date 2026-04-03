using System.Net;
using System.Net.Http.Json;
using AvailabilityService.Application.Clients;

namespace AvailabilityService.Infrastructure.Clients;

public class FlightApiClient : IFlightCapacityClient
{
    private readonly HttpClient _httpClient;

    public FlightApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FlightCapacityResult> GetCapacityAsync(string flightId, string authorizationHeader)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/flights/{flightId}");

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        }

        try
        {
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound || !response.IsSuccessStatusCode)
            {
                return new FlightCapacityResult();
            }

            var payload = await response.Content.ReadFromJsonAsync<FlightPayload>();

            if (payload is null)
            {
                return new FlightCapacityResult();
            }

            return new FlightCapacityResult
            {
                Success = true,
                AvailableSeatCount = payload.AvailableSeatCount
            };
        }
        catch (HttpRequestException)
        {
            return new FlightCapacityResult();
        }
        catch (TaskCanceledException)
        {
            return new FlightCapacityResult();
        }
    }

    private sealed class FlightPayload
    {
        public int AvailableSeatCount { get; set; }
    }
}
