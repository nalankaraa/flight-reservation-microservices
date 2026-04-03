using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ReservationService.Application.Dtos;
using ReservationService.Application.Services;
using System.Security.Claims;

namespace ReservationService.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _reservationService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _reservationService.GetMineAsync(userId);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var authorizationHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(authorizationHeader))
            return Unauthorized();

        var result = await _reservationService.CreateAsync(request, userId, authorizationHeader);

        if (!result.Success)
        {
            if (result.ErrorCode == "SeatConflict")
                return Conflict(result);

            if (result.ErrorCode == "AvailabilityUnavailable")
                return StatusCode(StatusCodes.Status503ServiceUnavailable, result);

            return BadRequest(result);
        }

        return Ok(result);
    }
}