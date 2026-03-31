using PaymentService.Application.Repositories;
using PaymentService.Domain.Entities;

namespace PaymentService.Tests;

public class FakePaymentRepository : IPaymentRepository
{
    public List<Payment> Payments { get; } = new();

    public Task AddAsync(Payment payment)
    {
        Payments.Add(payment);
        return Task.CompletedTask;
    }

    public Task<Payment?> GetByIdAsync(string id)
    {
        return Task.FromResult(Payments.FirstOrDefault(x => x.Id == id));
    }

    public Task UpdateAsync(Payment payment)
    {
        var existing = Payments.FirstOrDefault(x => x.Id == payment.Id);

        if (existing != null)
        {
            Payments.Remove(existing);
            Payments.Add(payment);
        }

        return Task.CompletedTask;
    }
}