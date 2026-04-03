using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Api.Controllers;
using NotificationService.Application.Dtos;
using NotificationService.Application.Services;

namespace NotificationService.Tests;

public class NotificationsControllerOwnershipTests
{
    [Fact]
    public async Task GetById_Should_Return_Forbid_When_Customer_Requests_Another_Users_Notification()
    {
        var service = new StubNotificationService();
        service.Notifications["notification-1"] = CreateNotification("notification-1", "user-2");
        var controller = CreateController(service, "Customer", "user-1");

        var result = await controller.GetById("notification-1");

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetByUserId_Should_Return_Forbid_When_Customer_Requests_Another_Users_Notifications()
    {
        var service = new StubNotificationService();
        var controller = CreateController(service, "Customer", "user-1");

        var result = await controller.GetByUserId("user-2");

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Send_Should_Return_Forbid_When_Customer_Updates_Another_Users_Notification()
    {
        var service = new StubNotificationService();
        service.Notifications["notification-1"] = CreateNotification("notification-1", "user-2");
        var controller = CreateController(service, "Customer", "user-1");

        var result = await controller.Send("notification-1");

        result.Should().BeOfType<ForbidResult>();
        service.SentNotificationIds.Should().BeEmpty();
    }

    [Fact]
    public async Task MarkAsRead_Should_Return_Forbid_When_Customer_Updates_Another_Users_Notification()
    {
        var service = new StubNotificationService();
        service.Notifications["notification-1"] = CreateNotification("notification-1", "user-2");
        var controller = CreateController(service, "Customer", "user-1");

        var result = await controller.MarkAsRead("notification-1");

        result.Should().BeOfType<ForbidResult>();
        service.ReadNotificationIds.Should().BeEmpty();
    }

    [Fact]
    public async Task Admin_Should_Be_Able_To_Access_And_Update_Any_Notification()
    {
        var service = new StubNotificationService();
        service.Notifications["notification-1"] = CreateNotification("notification-1", "user-2");
        var controller = CreateController(service, "Admin", "admin-1");

        var getResult = await controller.GetById("notification-1");
        var listResult = await controller.GetByUserId("user-2");
        var sendResult = await controller.Send("notification-1");
        var readResult = await controller.MarkAsRead("notification-1");

        getResult.Should().BeOfType<OkObjectResult>();
        listResult.Should().BeOfType<OkObjectResult>();
        sendResult.Should().BeOfType<OkObjectResult>();
        readResult.Should().BeOfType<OkObjectResult>();
        service.SentNotificationIds.Should().ContainSingle("notification-1");
        service.ReadNotificationIds.Should().ContainSingle("notification-1");
    }

    private static NotificationsController CreateController(INotificationService notificationService, string role, string userId)
    {
        var controller = new NotificationsController(notificationService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Role, role)
                    ], "TestAuth"))
                }
            }
        };

        return controller;
    }

    private static NotificationResponseDto CreateNotification(string id, string userId)
    {
        return new NotificationResponseDto
        {
            Id = id,
            UserId = userId,
            Title = "Payment Completed",
            Message = "Your payment has been completed.",
            Type = "PaymentCompleted",
            CreatedAtUtc = DateTime.UtcNow,
            IsRead = false,
            IsSent = false
        };
    }

    private sealed class StubNotificationService : INotificationService
    {
        public Dictionary<string, NotificationResponseDto> Notifications { get; } = new();
        public List<string> SentNotificationIds { get; } = [];
        public List<string> ReadNotificationIds { get; } = [];

        public Task<NotificationResponseDto> CreateAsync(CreateNotificationDto request)
        {
            throw new NotSupportedException();
        }

        public Task<NotificationResponseDto?> GetByIdAsync(string id)
        {
            Notifications.TryGetValue(id, out var notification);
            return Task.FromResult(notification);
        }

        public Task<List<NotificationResponseDto>> GetByUserIdAsync(string userId)
        {
            var result = Notifications.Values
                .Where(x => x.UserId == userId)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<bool> SendAsync(string id)
        {
            SentNotificationIds.Add(id);
            return Task.FromResult(true);
        }

        public Task<bool> MarkAsReadAsync(string id)
        {
            ReadNotificationIds.Add(id);
            return Task.FromResult(true);
        }
    }
}
