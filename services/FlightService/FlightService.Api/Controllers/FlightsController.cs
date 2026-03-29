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

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFlightDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _flightService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateFlightDto request)
    {
        var success = await _flightService.UpdateAsync(id, request);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _flightService.DeleteAsync(id);

        if (!success)
            return NotFound();

        return NoContent();
    }
}