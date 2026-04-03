using Dispatcher.Application.Forwarding;
using Dispatcher.Domain.Routing;
using FlightService.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api/flights")]
[Authorize]
public class FlightsGatewayController : DispatcherProxyControllerBase
{
    public FlightsGatewayController(IRequestForwarder requestForwarder, IRouteResolver routeResolver)
        : base(requestForwarder, routeResolver)
    {
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> GetAll()
    {
        return ForwardAsync("/api/flights", HttpMethods.Get);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> GetById(string id)
    {
        return ForwardAsync($"/api/flights/{id}", HttpMethods.Get);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Create([FromBody] CreateFlightDto request)
    {
        return ForwardAsync("/api/flights", HttpMethods.Post, request);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Update(string id, [FromBody] UpdateFlightDto request)
    {
        return ForwardAsync($"/api/flights/{id}", HttpMethods.Put, request);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Delete(string id)
    {
        return ForwardAsync($"/api/flights/{id}", HttpMethods.Delete);
    }
}
