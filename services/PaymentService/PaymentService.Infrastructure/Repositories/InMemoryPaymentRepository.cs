using PaymentService.Application.Repositories;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Repositories;

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly List<Payment> _payments = new();

    public Task AddAsync(Payment payment)
    {
        _payments.Add(payment);
        return Task.CompletedTask;
    }

    public Task<Payment?> GetByIdAsync(string id)
    {
        var payment = _payments.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(payment);
    }

    public Task UpdateAsync(Payment payment)
    {
        var existing = _payments.FirstOrDefault(x => x.Id == payment.Id);

        if (existing != null)
        {
            _payments.Remove(existing);
            _payments.Add(payment);
        }

        return Task.CompletedTask;
    }
}