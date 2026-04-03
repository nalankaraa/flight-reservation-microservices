using PaymentService.Application.Dtos;
using PaymentService.Application.Repositories;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;

    public PaymentService(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentResponseDto> CreateAsync(CreatePaymentDto request)
    {
        var payment = new Payment
        {
            ReservationId = request.ReservationId,
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

    public async Task<bool> CompleteAsync(string id)
    {
        var payment = await _repository.GetByIdAsync(id);

        if (payment is null)
            return false;

        if (payment.Status != "Pending")
            return false;

        payment.Status = "Completed";
        await _repository.UpdateAsync(payment);

        return true;
    }

    public async Task<bool> FailAsync(string id)
    {
        var payment = await _repository.GetByIdAsync(id);

        if (payment is null)
            return false;

        if (payment.Status != "Pending")
            return false;

        payment.Status = "Failed";
        await _repository.UpdateAsync(payment);

        return true;
    }

    private static PaymentResponseDto MapToDto(Payment payment)
    {
        return new PaymentResponseDto
        {
            Id = payment.Id,
            ReservationId = payment.ReservationId,
            Amount = payment.Amount,
            Status = payment.Status,
            CreatedAtUtc = payment.CreatedAtUtc
        };
    }
}