using FluentAssertions;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Repositories;

namespace NotificationService.Tests;

public class MongoNotificationRepositoryTests : IClassFixture<MongoNotificationRepositoryTestFixture>
{
    private readonly MongoNotificationRepositoryTestFixture _fixture;

    public MongoNotificationRepositoryTests(MongoNotificationRepositoryTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_Then_GetByIdAsync_Should_Return_Stored_Notification()
    {
        var repository = new MongoNotificationRepository(_fixture.CreateDatabase());
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "user-1",
            Title = "Reservation Created",
            Message = "Your reservation has been created.",
            Type = "ReservationCreated",
            CreatedAtUtc = DateTime.UtcNow,
            IsRead = false,
            IsSent = false
        };

        await repository.AddAsync(notification);

        var result = await repository.GetByIdAsync(notification.Id);

        result.Should().NotBeNull();
        result!.UserId.Should().Be("user-1");
        result.Title.Should().Be("Reservation Created");
        result.Type.Should().Be("ReservationCreated");
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Only_Matching_User_Notifications()
    {
        var repository = new MongoNotificationRepository(_fixture.CreateDatabase());

        await repository.AddAsync(new Notification
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "user-1",
            Title = "Payment Completed",
            Message = "Your payment has been completed.",
            Type = "PaymentCompleted",
            CreatedAtUtc = DateTime.UtcNow,
            IsRead = false,
            IsSent = false
        });

        await repository.AddAsync(new Notification
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "user-2",
            Title = "Payment Failed",
            Message = "Your payment has failed.",
            Type = "PaymentFailed",
            CreatedAtUtc = DateTime.UtcNow,
            IsRead = false,
            IsSent = false
        });

        var result = await repository.GetByUserIdAsync("user-1");

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be("user-1");
        result[0].Type.Should().Be("PaymentCompleted");
    }
}