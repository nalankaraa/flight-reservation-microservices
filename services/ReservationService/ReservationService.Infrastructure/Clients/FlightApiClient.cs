using System.Net;
using System.Net.Http.Json;
using ReservationService.Application.Clients;

namespace ReservationService.Infrastructure.Clients;

public class FlightApiClient : IFlightDetailsClient
{
    private readonly HttpClient _httpClient;

    public FlightApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FlightLookupResult> GetByIdAsync(string flightId, string authorizationHeader)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/flights/{flightId}");
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        try
        {
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new FlightLookupResult
                {
                    IsNotFound = true
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                return new FlightLookupResult
                {
                    IsServiceUnavailable = true
                };
            }

            var payload = await response.Content.ReadFromJsonAsync<FlightPayload>();

            if (payload is null)
            {
                return new FlightLookupResult
                {
                    IsServiceUnavailable = true
                };
            }

            return new FlightLookupResult
            {
                Success = true,
                Price = payload.Price
            };
        }
        catch (HttpRequestException)
        {
            return new FlightLookupResult
            {
                IsServiceUnavailable = true
            };
        }
        catch (TaskCanceledException)
        {
            return new FlightLookupResult
            {
                IsServiceUnavailable = true
            };
        }
    }

    private sealed class FlightPayload
    {
        public decimal Price { get; set; }
    }
}
