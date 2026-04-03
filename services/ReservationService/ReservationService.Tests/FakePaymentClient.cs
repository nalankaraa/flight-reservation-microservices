using ReservationService.Application.Clients;

namespace ReservationService.Tests;

public class FakePaymentClient : IPaymentClient
{
    public bool ShouldBeUnavailable { get; set; }
    public int CreateCallCount { get; private set; }

    public Task<PaymentCreationResult> CreateAsync(string reservationId, string userId, decimal amount, string authorizationHeader)
    {
        CreateCallCount++;

        if (ShouldBeUnavailable)
        {
            return Task.FromResult(new PaymentCreationResult
            {
                IsServiceUnavailable = true
            });
        }

        return Task.FromResult(new PaymentCreationResult
        {
            Success = true,
            PaymentId = "payment-1",
            Status = "Pending"
        });
    }
}
