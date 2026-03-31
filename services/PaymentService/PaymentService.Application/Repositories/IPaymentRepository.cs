using PaymentService.Domain.Entities;

namespace PaymentService.Application.Repositories;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task<Payment?> GetByIdAsync(string id);
    Task UpdateAsync(Payment payment);
}