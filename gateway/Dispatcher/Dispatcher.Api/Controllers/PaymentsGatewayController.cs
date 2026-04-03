using Dispatcher.Application.Forwarding;
using Dispatcher.Domain.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Dtos;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsGatewayController : DispatcherProxyControllerBase
{
    public PaymentsGatewayController(IRequestForwarder requestForwarder, IRouteResolver routeResolver)
        : base(requestForwarder, routeResolver)
    {
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> Create([FromBody] CreatePaymentDto request)
    {
        return ForwardAsync("/api/payments", HttpMethods.Post, request);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> GetById(string id)
    {
        return ForwardAsync($"/api/payments/{id}", HttpMethods.Get);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> UpdateStatus(string id, [FromBody] UpdatePaymentStatusDto request)
    {
        return ForwardAsync($"/api/payments/{id}", HttpMethods.Patch, request);
    }
}
