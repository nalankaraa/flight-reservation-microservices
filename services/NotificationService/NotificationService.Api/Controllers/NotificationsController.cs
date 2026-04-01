using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.Services;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateNotificationDto request)
    {
        var result = await _notificationService.CreateAsync(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _notificationService.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(string userId)
    {
        var result = await _notificationService.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> Send(string id)
    {
        var success = await _notificationService.SendAsync(id);

        if (!success)
            return NotFound();

        return NoContent();
    }
}