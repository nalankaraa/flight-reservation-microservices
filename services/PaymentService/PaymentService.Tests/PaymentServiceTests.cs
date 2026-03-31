using FluentAssertions;
using PaymentService.Application.Dtos;
using Xunit;

namespace PaymentService.Tests;

public class PaymentServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Create_Payment()
    {
        // Arrange
        var repository = new FakePaymentRepository();
        var service = new PaymentService.Application.Services.PaymentService(repository);

        var request = new CreatePaymentDto
        {
            ReservationId = "reservation-1",
            Amount = 2500
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ReservationId.Should().Be("reservation-1");
        result.Amount.Should().Be(2500);
        result.Status.Should().Be("Pending");
        repository.Payments.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Payment_When_It_Exists()
    {
        // Arrange
        var repository = new FakePaymentRepository();
        var service = new PaymentService.Application.Services.PaymentService(repository);

        var created = await service.CreateAsync(new CreatePaymentDto
        {
            ReservationId = "reservation-1",
            Amount = 2500
        });

        // Act
        var result = await service.GetByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Payment_Does_Not_Exist()
    {
        // Arrange
        var repository = new FakePaymentRepository();
        var service = new PaymentService.Application.Services.PaymentService(repository);

        // Act
        var result = await service.GetByIdAsync("missing-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_Should_Set_Status_To_Completed()
    {
        // Arrange
        var repository = new FakePaymentRepository();
        var service = new PaymentService.Application.Services.PaymentService(repository);

        var created = await service.CreateAsync(new CreatePaymentDto
        {
            ReservationId = "reservation-1",
            Amount = 2500
        });

        // Act
        var success = await service.CompleteAsync(created.Id);
        var updated = await service.GetByIdAsync(created.Id);

        // Assert
        success.Should().BeTrue();
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task FailAsync_Should_Set_Status_To_Failed()
    {
        // Arrange
        var repository = new FakePaymentRepository();
        var service = new PaymentService.Application.Services.PaymentService(repository);

        var created = await service.CreateAsync(new CreatePaymentDto
        {
            ReservationId = "reservation-1",
            Amount = 2500
        });

        // Act
        var success = await service.FailAsync(created.Id);
        var updated = await service.GetByIdAsync(created.Id);

        // Assert
        success.Should().BeTrue();
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("Failed");
    }

    [Fact]
    public async Task CompleteAsync_Should_Return_False_When_Payment_Is_Already_Completed()
    {
        // Arrange
        var repository = new FakePaymentRepository();
        var service = new PaymentService.Application.Services.PaymentService(repository);

        var created = await service.CreateAsync(new CreatePaymentDto
        {
            ReservationId = "reservation-1",
            Amount = 2500
        });

        await service.CompleteAsync(created.Id);

        // Act
        var secondAttempt = await service.CompleteAsync(created.Id);

        // Assert
        secondAttempt.Should().BeFalse();
    }

    [Fact]
    public async Task FailAsync_Should_Return_False_When_Payment_Is_Already_Completed()
    {
        // Arrange
        var repository = new FakePaymentRepository();
        var service = new PaymentService.Application.Services.PaymentService(repository);

        var created = await service.CreateAsync(new CreatePaymentDto
        {
            ReservationId = "reservation-1",
            Amount = 2500
        });

        await service.CompleteAsync(created.Id);

        // Act
        var failAttempt = await service.FailAsync(created.Id);

        // Assert
        failAttempt.Should().BeFalse();
    }
}