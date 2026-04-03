using AvailabilityService.Application.Dtos;
using AvailabilityService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AvailabilityService.Api.Controllers;

[ApiController]
[Route("api/availability")]
[Authorize]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpGet("{flightId}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetAvailability(string flightId)
    {
        var result = await _availabilityService.GetAvailabilityAsync(flightId);
        return Ok(result);
    }

    [HttpGet("{flightId}/seats")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetSeats(string flightId)
    {
        var result = await _availabilityService.GetSeatsAsync(flightId);
        return Ok(result);
    }

    [HttpPost("{flightId}/lock-seat")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> LockSeat(string flightId, [FromBody] LockSeatRequestDto request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _availabilityService.LockSeatAsync(flightId, request, userId);

        if (result is null)
            return Conflict("Seat is already locked or reserved.");

        return Ok(result);
    }

    [HttpPost("{flightId}/confirm-seat")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> ConfirmSeat(string flightId, [FromBody] ConfirmSeatRequestDto request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _availabilityService.ConfirmSeatAsync(flightId, request.SeatNumber, userId);

        if (result is null)
            return Conflict("Seat cannot be confirmed.");

        return Ok(result);
    }

    [HttpPost("{flightId}/release-seat")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> ReleaseSeat(string flightId, [FromBody] ReleaseSeatRequestDto request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var allowAnyUser = User.IsInRole("Admin");
        var success = await _availabilityService.ReleaseSeatAsync(flightId, request, userId, allowAnyUser);

        if (!success)
            return Conflict("Seat cannot be released.");

        return NoContent();
    }
}