using AuthService.Application.Dtos;
using Dispatcher.Application.Forwarding;
using Dispatcher.Domain.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthGatewayController : DispatcherProxyControllerBase
{
    public AuthGatewayController(IRequestForwarder requestForwarder, IRouteResolver routeResolver)
        : base(requestForwarder, routeResolver)
    {
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        return ForwardAsync("/api/auth/register", HttpMethods.Post, request);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        return ForwardAsync("/api/auth/login", HttpMethods.Post, request);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Admin,Customer")]
    public Task<IActionResult> Me()
    {
        return ForwardAsync("/api/auth/me", HttpMethods.Get);
    }
}
