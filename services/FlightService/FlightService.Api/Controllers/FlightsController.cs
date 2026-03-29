using FlightService.Application.Dtos;
using FlightService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightService.Api.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;

    public FlightsController(IFlightService flightService)
    {
        _flightService = flightService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _flightService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _flightService.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFlightDto request)
    {
        var result = await _flightService.CreateAsync(request);
        return Ok(result);
    }
}