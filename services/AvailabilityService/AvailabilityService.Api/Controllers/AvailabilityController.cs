using AvailabilityService.Application.Dtos;
using AvailabilityService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AvailabilityService.Api.Controllers;

[ApiController]
[Route("api/availability/holds")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateHold([FromBody] CreateSeatHoldDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _availabilityService.CreateHoldAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _availabilityService.GetHoldByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(string id)
    {
        var success = await _availabilityService.ConfirmHoldAsync(id);

        if (!success)
            return BadRequest("Hold cannot be confirmed.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(string id)
    {
        var success = await _availabilityService.CancelHoldAsync(id);

        if (!success)
            return BadRequest("Hold cannot be cancelled.");

        return NoContent();
    }
}