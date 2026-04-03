using System.Net;
using System.Net.Http.Json;
using ReservationService.Application.Clients;

namespace ReservationService.Infrastructure.Clients;

public class AvailabilityApiClient : ISeatAvailabilityClient
{
    private readonly HttpClient _httpClient;

    public AvailabilityApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SeatLockResult> LockSeatAsync(string flightId, string seatNumber, int holdMinutes, string authorizationHeader)
    {
<<<<<<< Updated upstream
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/availability/{flightId}/seats/{Uri.EscapeDataString(seatNumber)}/hold");
=======
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/availability/{flightId}/seats/{Uri.EscapeDataString(seatNumber)}/hold");

>>>>>>> Stashed changes
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        request.Content = JsonContent.Create(new
        {
            HoldMinutes = holdMinutes
        });

<<<<<<< Updated upstream
        try
        {
            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return new SeatLockResult
                {
                    Success = true
                };
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return new SeatLockResult
                {
                    IsConflict = true
                };
            }

            return new SeatLockResult
            {
                IsServiceUnavailable = true
            };
        }
        catch (HttpRequestException)
        {
            return new SeatLockResult
            {
                IsServiceUnavailable = true
            };
        }
        catch (TaskCanceledException)
        {
            return new SeatLockResult
            {
                IsServiceUnavailable = true
            };
        }
=======
        return await SendAsync(request);
>>>>>>> Stashed changes
    }

    public async Task<SeatLockResult> ConfirmSeatAsync(string flightId, string seatNumber, string authorizationHeader)
    {
<<<<<<< Updated upstream
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/availability/{flightId}/seats/{Uri.EscapeDataString(seatNumber)}/reservation");
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        try
        {
            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return new SeatLockResult
                {
                    Success = true
                };
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return new SeatLockResult
                {
                    IsConflict = true
                };
            }

            return new SeatLockResult
            {
                IsServiceUnavailable = true
            };
        }
        catch (HttpRequestException)
        {
            return new SeatLockResult
            {
                IsServiceUnavailable = true
            };
        }
        catch (TaskCanceledException)
        {
            return new SeatLockResult
            {
                IsServiceUnavailable = true
            };
        }
=======
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/availability/{flightId}/seats/{Uri.EscapeDataString(seatNumber)}/reservation");

        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        return await SendAsync(request);
>>>>>>> Stashed changes
    }

    public async Task ReleaseSeatAsync(string flightId, string seatNumber, string authorizationHeader)
    {
<<<<<<< Updated upstream
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/availability/{flightId}/seats/{Uri.EscapeDataString(seatNumber)}/hold");
=======
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/availability/{flightId}/seats/{Uri.EscapeDataString(seatNumber)}/hold");

>>>>>>> Stashed changes
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        try
        {
            using var _ = await _httpClient.SendAsync(request);
        }
        catch (HttpRequestException)
        {
        }
        catch (TaskCanceledException)
        {
        }
    }
<<<<<<< Updated upstream
=======

    private async Task<SeatLockResult> SendAsync(HttpRequestMessage request)
    {
        try
        {
            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return new SeatLockResult
                {
                    Success = true
                };
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return new SeatLockResult
                {
                    IsConflict = true
                };
            }

            return new SeatLockResult
            {
                IsServiceUnavailable = true
            };
        }
        catch (HttpRequestException)
        {
            return new SeatLockResult
            {
                IsServiceUnavailable = true
            };
        }
        catch (TaskCanceledException)
        {
            return new SeatLockResult
            {
                IsServiceUnavailable = true
            };
        }
    }
>>>>>>> Stashed changes
}
