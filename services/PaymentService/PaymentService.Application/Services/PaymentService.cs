using PaymentService.Application.Dtos;
using PaymentService.Application.Clients;
using PaymentService.Application.Repositories;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly INotificationClient _notificationClient;

    public PaymentService(IPaymentRepository repository, INotificationClient notificationClient)
    {
        _repository = repository;
        _notificationClient = notificationClient;
    }

    public async Task<PaymentResponseDto> CreateAsync(CreatePaymentDto request)
    {
        var payment = new Payment
        {
            ReservationId = request.ReservationId,
            UserId = request.UserId,
            Amount = request.Amount,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        };

        await _repository.AddAsync(payment);

        return MapToDto(payment);
    }

    public async Task<PaymentResponseDto?> GetByIdAsync(string id)
    {
        var payment = await _repository.GetByIdAsync(id);

        if (payment is null)
            return null;

        return MapToDto(payment);
    }

    public async Task<bool> CompleteAsync(string id, string authorizationHeader)
    {
        var payment = await _repository.GetByIdAsync(id);

        if (payment is null)
            return false;

        if (payment.Status != "Pending")
            return false;

        payment.Status = "Completed";
        await _repository.UpdateAsync(payment);
        await TryCreateNotificationAsync(
            payment.UserId,
            "Payment Completed",
            $"Payment for reservation {payment.ReservationId} has been completed.",
            "PaymentCompleted",
            authorizationHeader);

        return true;
    }

    public async Task<bool> FailAsync(string id, string authorizationHeader)
    {
        var payment = await _repository.GetByIdAsync(id);

        if (payment is null)
            return false;

        if (payment.Status != "Pending")
            return false;

        payment.Status = "Failed";
        await _repository.UpdateAsync(payment);
        await TryCreateNotificationAsync(
            payment.UserId,
            "Payment Failed",
            $"Payment for reservation {payment.ReservationId} has failed.",
            "PaymentFailed",
            authorizationHeader);

        return true;
    }

    private static PaymentResponseDto MapToDto(Payment payment)
    {
        return new PaymentResponseDto
        {
            Id = payment.Id,
            ReservationId = payment.ReservationId,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Status = payment.Status,
            CreatedAtUtc = payment.CreatedAtUtc
        };
    }

    private async Task TryCreateNotificationAsync(
        string userId,
        string title,
        string message,
        string type,
        string authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return;

        try
        {
            await _notificationClient.CreateAsync(userId, title, message, type, authorizationHeader);
        }
        catch (HttpRequestException)
        {
        }
        catch (TaskCanceledException)
        {
        }
    }
}
