using FluentAssertions;
using NotificationService.Application.Dtos;
using Xunit;

namespace NotificationService.Tests;

public class NotificationServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Create_Notification()
    {
        var repository = new FakeNotificationRepository();
        var service = new NotificationService.Application.Services.NotificationService(repository);

        var request = new CreateNotificationDto
        {
            UserId = "user-1",
            Title = "Reservation Created",
            Message = "Your reservation has been created.",
            Type = "ReservationCreated"
        };

        var result = await service.CreateAsync(request);

        result.Should().NotBeNull();
        result.UserId.Should().Be("user-1");
        result.Title.Should().Be("Reservation Created");
        result.Type.Should().Be("ReservationCreated");
        result.IsSent.Should().BeFalse();
        repository.Notifications.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Notification_When_It_Exists()
    {
        var repository = new FakeNotificationRepository();
        var service = new NotificationService.Application.Services.NotificationService(repository);

        var created = await service.CreateAsync(new CreateNotificationDto
        {
            UserId = "user-1",
            Title = "Reservation Created",
            Message = "Your reservation has been created.",
            Type = "ReservationCreated"
        });

        var result = await service.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_User_Notifications()
    {
        var repository = new FakeNotificationRepository();
        var service = new NotificationService.Application.Services.NotificationService(repository);

        await service.CreateAsync(new CreateNotificationDto
        {
            UserId = "user-1",
            Title = "Reservation Created",
            Message = "Your reservation has been created.",
            Type = "ReservationCreated"
        });

        await service.CreateAsync(new CreateNotificationDto
        {
            UserId = "user-1",
            Title = "Payment Completed",
            Message = "Your payment has been completed.",
            Type = "PaymentCompleted"
        });

        var result = await service.GetByUserIdAsync("user-1");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SendAsync_Should_Set_IsSent_To_True()
    {
        var repository = new FakeNotificationRepository();
        var service = new NotificationService.Application.Services.NotificationService(repository);

        var created = await service.CreateAsync(new CreateNotificationDto
        {
            UserId = "user-1",
            Title = "Reservation Created",
            Message = "Your reservation has been created.",
            Type = "ReservationCreated"
        });

        var success = await service.SendAsync(created.Id);
        var updated = await service.GetByIdAsync(created.Id);

        success.Should().BeTrue();
        updated.Should().NotBeNull();
        updated!.IsSent.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Notification_Is_Already_Sent()
    {
        var repository = new FakeNotificationRepository();
        var service = new NotificationService.Application.Services.NotificationService(repository);

        var created = await service.CreateAsync(new CreateNotificationDto
        {
            UserId = "user-1",
            Title = "Reservation Created",
            Message = "Your reservation has been created.",
            Type = "ReservationCreated"
        });

        await service.SendAsync(created.Id);

        var secondAttempt = await service.SendAsync(created.Id);

        secondAttempt.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsReadAsync_Should_Set_IsRead_To_True()
    {
        var repository = new FakeNotificationRepository();
        var service = new NotificationService.Application.Services.NotificationService(repository);

        var created = await service.CreateAsync(new CreateNotificationDto
        {
            UserId = "user-1",
            Title = "Payment Completed",
            Message = "Your payment has been completed.",
            Type = "PaymentCompleted"
        });

        var success = await service.MarkAsReadAsync(created.Id);
        var updated = await service.GetByIdAsync(created.Id);

        success.Should().BeTrue();
        updated.Should().NotBeNull();
        updated!.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsReadAsync_Should_Return_False_When_Notification_Is_Already_Read()
    {
        var repository = new FakeNotificationRepository();
        var service = new NotificationService.Application.Services.NotificationService(repository);

        var created = await service.CreateAsync(new CreateNotificationDto
        {
            UserId = "user-1",
            Title = "Payment Completed",
            Message = "Your payment has been completed.",
            Type = "PaymentCompleted"
        });

        await service.MarkAsReadAsync(created.Id);

        var secondAttempt = await service.MarkAsReadAsync(created.Id);

        secondAttempt.Should().BeFalse();
    }
}