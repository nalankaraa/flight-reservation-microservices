using FluentAssertions;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.Tests;

public class MongoPaymentRepositoryTests : IClassFixture<MongoPaymentRepositoryTestFixture>
{
    private readonly MongoPaymentRepositoryTestFixture _fixture;

    public MongoPaymentRepositoryTests(MongoPaymentRepositoryTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_Then_GetByIdAsync_Should_Return_Persisted_Payment_With_UserId()
    {
        var repository = new MongoPaymentRepository(_fixture.CreateDatabase());
        var payment = new Payment
        {
            Id = Guid.NewGuid().ToString(),
            ReservationId = "reservation-1",
            UserId = "user-1",
            Amount = 2500,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        };

        await repository.AddAsync(payment);

        var result = await repository.GetByIdAsync(payment.Id);

        result.Should().NotBeNull();
        result!.ReservationId.Should().Be("reservation-1");
        result.UserId.Should().Be("user-1");
        result.Amount.Should().Be(2500);
    }

    [Fact]
    public async Task UpdateAsync_Should_Persist_Modified_Status()
    {
        var repository = new MongoPaymentRepository(_fixture.CreateDatabase());
        var payment = new Payment
        {
            Id = Guid.NewGuid().ToString(),
            ReservationId = "reservation-2",
            UserId = "user-2",
            Amount = 1500,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        };

        await repository.AddAsync(payment);
        payment.Status = "Completed";

        await repository.UpdateAsync(payment);

        var result = await repository.GetByIdAsync(payment.Id);

        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.UserId.Should().Be("user-2");
    }
}