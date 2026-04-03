namespace PaymentService.Application.Clients;

public interface INotificationClient
{
    Task CreateAsync(string userId, string title, string message, string type, string authorizationHeader);
}
