using PaymentService.Application.Clients;

namespace PaymentService.Tests;

public class FakeNotificationClient : INotificationClient
{
    public int CreateCallCount { get; private set; }
    public string? LastType { get; private set; }

    public Task CreateAsync(string userId, string title, string message, string type, string authorizationHeader)
    {
        CreateCallCount++;
        LastType = type;
        return Task.CompletedTask;
    }
}
