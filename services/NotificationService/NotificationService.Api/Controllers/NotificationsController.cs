using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotificationService.Application.Dtos;
using NotificationService.Application.Services;

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

        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetByUserId(string userId)
    {
        var result = await _notificationService.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Send(string id)
    {
        var success = await _notificationService.SendAsync(id);

        if (!success)
            return NotFound();

        return Ok(new { message = "Notification marked as sent." });
    }

    [HttpPost("{id}/read")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var success = await _notificationService.MarkAsReadAsync(id);

        if (!success)
            return NotFound();

        return Ok(new { message = "Notification marked as read." });
    }
}
