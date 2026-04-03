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

    [HttpPost("{flightId}/lock-seat")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> LockSeat(string flightId, [FromBody] LockSeatRequestDto request)
    {
        return ForwardAsync($"/api/availability/{flightId}/lock-seat", HttpMethods.Post, request);
    }

    [HttpPost("{flightId}/confirm-seat")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> ConfirmSeat(string flightId, [FromBody] ConfirmSeatRequestDto request)
    {
        return ForwardAsync($"/api/availability/{flightId}/confirm-seat", HttpMethods.Post, request);
    }

    [HttpPost("{flightId}/release-seat")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> ReleaseSeat(string flightId, [FromBody] ReleaseSeatRequestDto request)
    {
        return ForwardAsync($"/api/availability/{flightId}/release-seat", HttpMethods.Post, request);
    }
}
