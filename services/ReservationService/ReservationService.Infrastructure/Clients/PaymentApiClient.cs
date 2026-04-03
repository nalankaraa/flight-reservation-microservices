using System.Net.Http.Json;
using ReservationService.Application.Clients;

namespace ReservationService.Infrastructure.Clients;

public class PaymentApiClient : IPaymentClient
{
    private readonly HttpClient _httpClient;

    public PaymentApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentCreationResult> CreateAsync(string reservationId, string userId, decimal amount, string authorizationHeader)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/payments");
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        request.Content = JsonContent.Create(new
        {
            ReservationId = reservationId,
            UserId = userId,
            Amount = amount
        });

        try
        {
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentCreationResult
                {
                    IsServiceUnavailable = true
                };
            }

            var payload = await response.Content.ReadFromJsonAsync<PaymentPayload>();

            if (payload is null)
            {
                return new PaymentCreationResult
                {
                    IsServiceUnavailable = true
                };
            }

            return new PaymentCreationResult
            {
                Success = true,
                PaymentId = payload.Id,
                Status = payload.Status
            };
        }
        catch (HttpRequestException)
        {
            return new PaymentCreationResult
            {
                IsServiceUnavailable = true
            };
        }
        catch (TaskCanceledException)
        {
            return new PaymentCreationResult
            {
                IsServiceUnavailable = true
            };
        }
    }

    private sealed class PaymentPayload
    {
        public string Id { get; set; } = default!;
        public string Status { get; set; } = default!;
    }
}
