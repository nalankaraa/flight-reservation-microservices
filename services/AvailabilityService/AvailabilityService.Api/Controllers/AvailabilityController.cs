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

    [HttpPut("{flightId}/seats/{seatNumber}/hold")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> UpsertSeatHold(string flightId, string seatNumber, [FromBody] UpdateSeatHoldDto request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _availabilityService.LockSeatAsync(flightId, new LockSeatRequestDto
        {
            SeatNumber = seatNumber,
            HoldMinutes = request.HoldMinutes
        }, userId);

        if (result is null)
            return Conflict("Seat is already locked or reserved.");

        return Ok(result);
    }

    [HttpPut("{flightId}/seats/{seatNumber}/reservation")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> ConfirmSeatReservation(string flightId, string seatNumber)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _availabilityService.ConfirmSeatAsync(flightId, seatNumber, userId);

        if (result is null)
            return Conflict("Seat cannot be confirmed.");

        return Ok(result);
    }

    [HttpDelete("{flightId}/seats/{seatNumber}/hold")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> DeleteSeatHold(string flightId, string seatNumber)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var allowAnyUser = User.IsInRole("Admin");
        var success = await _availabilityService.ReleaseSeatAsync(flightId, new ReleaseSeatRequestDto
        {
            SeatNumber = seatNumber
        }, userId, allowAnyUser);

        if (!success)
            return Conflict("Seat cannot be released.");

        return NoContent();
    }
}
