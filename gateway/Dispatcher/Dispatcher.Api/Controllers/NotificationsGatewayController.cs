using Dispatcher.Application.Forwarding;
using Dispatcher.Domain.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsGatewayController : DispatcherProxyControllerBase
{
    public NotificationsGatewayController(IRequestForwarder requestForwarder, IRouteResolver routeResolver)
        : base(requestForwarder, routeResolver)
    {
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> Create([FromBody] CreateNotificationDto request)
    {
        return ForwardAsync("/api/notifications", HttpMethods.Post, request);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> GetById(string id)
    {
        return ForwardAsync($"/api/notifications/{id}", HttpMethods.Get);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> GetByUserId(string userId)
    {
        return ForwardAsync($"/api/notifications/user/{userId}", HttpMethods.Get);
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> Send(string id)
    {
        return ForwardAsync($"/api/notifications/{id}/send", HttpMethods.Post);
    }

    [HttpPost("{id}/read")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> MarkAsRead(string id)
    {
        return ForwardAsync($"/api/notifications/{id}/read", HttpMethods.Post);
    }
}
