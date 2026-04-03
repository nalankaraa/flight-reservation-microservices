using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotificationService.Application.Dtos;
using NotificationService.Application.Services;
using System.Security.Claims;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Create([FromBody] CreateNotificationDto request)
    {
        var result = await _notificationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _notificationService.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        if (!CanAccessNotification(result))
            return Forbid();

        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetByUserId(string userId)
    {
        if (!CanAccessUserNotifications(userId))
            return Forbid();

        var result = await _notificationService.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Send(string id)
    {
        var notification = await _notificationService.GetByIdAsync(id);

        if (notification is null)
            return NotFound();

        if (!CanAccessNotification(notification))
            return Forbid();

        var success = await _notificationService.SendAsync(id);

        if (!success)
            return NotFound();

        return Ok(new { message = "Notification marked as sent." });
    }

    [HttpPost("{id}/read")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var notification = await _notificationService.GetByIdAsync(id);

        if (notification is null)
            return NotFound();

        if (!CanAccessNotification(notification))
            return Forbid();

        var success = await _notificationService.MarkAsReadAsync(id);

        if (!success)
            return NotFound();

        return Ok(new { message = "Notification marked as read." });
    }

    private bool CanAccessNotification(NotificationResponseDto notification)
    {
        if (User.IsInRole("Admin"))
            return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return !string.IsNullOrWhiteSpace(userId)
               && string.Equals(notification.UserId, userId, StringComparison.Ordinal);
    }

    private bool CanAccessUserNotifications(string requestedUserId)
    {
        if (User.IsInRole("Admin"))
            return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return !string.IsNullOrWhiteSpace(userId)
               && string.Equals(requestedUserId, userId, StringComparison.Ordinal);
    }
}
