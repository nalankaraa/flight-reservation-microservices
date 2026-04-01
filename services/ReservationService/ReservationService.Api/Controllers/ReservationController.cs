using Microsoft.AspNetCore.Mvc;
using ReservationService.Application.Dtos;
using ReservationService.Application.Services;

namespace ReservationService.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _reservationService.GetAllAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _reservationService.CreateAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}