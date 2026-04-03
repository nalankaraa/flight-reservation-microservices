using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ReservationService.Application.Dtos;
using ReservationService.Application.Services;
using System.Security.Claims;
using BuildingBlocks.Application.Hateoas;

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
        result.ForEach(AttachLinks);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _reservationService.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && result.UserId != userId)
            return Forbid();

        AttachLinks(result);
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
        result.ForEach(AttachLinks);
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
            {
                AttachFailureLinks(result);
                return Conflict(result);
            }

            if (result.ErrorCode == "AvailabilityUnavailable")
            {
                AttachFailureLinks(result);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, result);
            }

            AttachFailureLinks(result);
            return BadRequest(result);
        }

        AttachLinks(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    private static void AttachLinks(ReservationResponseDto reservation)
    {
        if (string.IsNullOrWhiteSpace(reservation.Id))
            return;

        reservation.Links =
        [
            new LinkDto { Rel = "self", Href = $"/api/reservations/{reservation.Id}", Method = "GET" },
            new LinkDto { Rel = "flight", Href = $"/api/flights/{reservation.FlightId}", Method = "GET" },
            new LinkDto { Rel = "payment", Href = "/api/payments", Method = "POST" },
            new LinkDto { Rel = "my-reservations", Href = "/api/reservations/my", Method = "GET" }
        ];
    }

    private static void AttachFailureLinks(ReservationResponseDto reservation)
    {
        reservation.Links =
        [
            new LinkDto { Rel = "flight", Href = $"/api/flights/{reservation.FlightId}", Method = "GET" },
            new LinkDto { Rel = "availability", Href = $"/api/availability/{reservation.FlightId}/seats", Method = "GET" }
        ];
    }
}
