using AvailabilityService.Application.Dtos;
using Dispatcher.Application.Forwarding;
using Dispatcher.Domain.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api/availability")]
[Authorize]
public class AvailabilityGatewayController : DispatcherProxyControllerBase
{
    public AvailabilityGatewayController(IRequestForwarder requestForwarder, IRouteResolver routeResolver)
        : base(requestForwarder, routeResolver)
    {
    }

    [HttpGet("{flightId}")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> GetAvailability(string flightId)
    {
        return ForwardAsync($"/api/availability/{flightId}", HttpMethods.Get);
    }

    [HttpGet("{flightId}/seats")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> GetSeats(string flightId)
    {
        return ForwardAsync($"/api/availability/{flightId}/seats", HttpMethods.Get);
    }

    [HttpPut("{flightId}/seats/{seatNumber}/hold")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> UpsertSeatHold(string flightId, string seatNumber, [FromBody] UpdateSeatHoldDto request)
    {
        return ForwardAsync($"/api/availability/{flightId}/seats/{seatNumber}/hold", HttpMethods.Put, request);
    }

    [HttpPut("{flightId}/seats/{seatNumber}/reservation")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> ConfirmSeatReservation(string flightId, string seatNumber)
    {
        return ForwardAsync($"/api/availability/{flightId}/seats/{seatNumber}/reservation", HttpMethods.Put);
    }

    [HttpDelete("{flightId}/seats/{seatNumber}/hold")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> DeleteSeatHold(string flightId, string seatNumber)
    {
        return ForwardAsync($"/api/availability/{flightId}/seats/{seatNumber}/hold", HttpMethods.Delete);
    }
}
