using FlightService.Application.Dtos;
using FlightService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Application.Hateoas;

namespace FlightService.Api.Controllers;

[ApiController]
[Route("api/flights")]
[Authorize]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;

    public FlightsController(IFlightService flightService)
    {
        _flightService = flightService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _flightService.GetAllAsync();
        result.ForEach(AttachLinks);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _flightService.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        AttachLinks(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateFlightDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _flightService.CreateAsync(request);

        AttachLinks(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string id, UpdateFlightDto request)
    {
        var success = await _flightService.UpdateAsync(id, request);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _flightService.DeleteAsync(id);

        if (!success)
            return NotFound();

        return NoContent();
    }

    private void AttachLinks(FlightResponseDto flight)
    {
        flight.Links =
        [
            CreateLink("self", nameof(GetById), flight.Id, "GET"),
            new LinkDto { Rel = "availability", Href = $"/api/availability/{flight.Id}", Method = "GET" },
            new LinkDto { Rel = "seats", Href = $"/api/availability/{flight.Id}/seats", Method = "GET" }
        ];

        if (User.IsInRole("Admin"))
        {
            flight.Links.Add(new LinkDto { Rel = "update", Href = $"/api/flights/{flight.Id}", Method = "PUT" });
            flight.Links.Add(new LinkDto { Rel = "delete", Href = $"/api/flights/{flight.Id}", Method = "DELETE" });
        }
    }

    private LinkDto CreateLink(string rel, string actionName, string id, string method)
    {
        var href = Url.Action(actionName, new { id }) ?? $"/api/flights/{id}";
        return new LinkDto { Rel = rel, Href = href, Method = method };
    }
}
