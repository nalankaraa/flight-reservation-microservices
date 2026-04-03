using System.Net.Http.Json;
using PaymentService.Application.Clients;

namespace PaymentService.Infrastructure.Clients;

public class NotificationApiClient : INotificationClient
{
    private readonly HttpClient _httpClient;

    public NotificationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CreateAsync(string userId, string title, string message, string type, string authorizationHeader)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/notifications");
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        request.Content = JsonContent.Create(new
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type
        });

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
