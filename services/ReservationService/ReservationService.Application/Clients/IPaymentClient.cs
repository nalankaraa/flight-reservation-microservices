namespace ReservationService.Application.Clients;

public interface IPaymentClient
{
    Task<PaymentCreationResult> CreateAsync(string reservationId, string userId, decimal amount, string authorizationHeader);
}
