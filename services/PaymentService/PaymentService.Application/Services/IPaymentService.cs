using PaymentService.Application.Dtos;

namespace PaymentService.Application.Services;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreateAsync(CreatePaymentDto request);
    Task<PaymentResponseDto?> GetByIdAsync(string id);
    Task<bool> CompleteAsync(string id);
    Task<bool> FailAsync(string id);
}