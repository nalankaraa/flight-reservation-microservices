using Dispatcher.Application.Forwarding;
using Dispatcher.Domain.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationService.Application.Dtos;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsGatewayController : DispatcherProxyControllerBase
{
    public ReservationsGatewayController(IRequestForwarder requestForwarder, IRouteResolver routeResolver)
        : base(requestForwarder, routeResolver)
    {
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> GetAll()
    {
        return ForwardAsync("/api/reservations", HttpMethods.Get);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> GetById(string id)
    {
        return ForwardAsync($"/api/reservations/{id}", HttpMethods.Get);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> GetMine()
    {
        return ForwardAsync("/api/reservations/my", HttpMethods.Get);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> Create([FromBody] CreateReservationDto request)
    {
        return ForwardAsync("/api/reservations", HttpMethods.Post, request);
    }
}
