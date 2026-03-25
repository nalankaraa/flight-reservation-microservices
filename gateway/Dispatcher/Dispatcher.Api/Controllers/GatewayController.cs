using Microsoft.AspNetCore.Mvc;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api")]
public class GatewayController : ControllerBase
{
    [HttpGet("flights")]
    public IActionResult GetFlights()
    {
        return Ok("Forwarded to FlightService");
    }

    [HttpPost("flights")]
    public IActionResult PostFlights()
    {
        return Ok("Forwarded to FlightService");
    }
}