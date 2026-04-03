using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentService.Application.Dtos;
using PaymentService.Application.Services;
using BuildingBlocks.Application.Hateoas;
using System.Security.Claims;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (!User.IsInRole("Admin"))
        {
            request.UserId = userId;
        }

        var result = await _paymentService.CreateAsync(request);

        AttachLinks(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _paymentService.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (!isAdmin && result.UserId != userId)
            return Forbid();

        AttachLinks(result);
        return Ok(result);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdatePaymentStatusDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var payment = await _paymentService.GetByIdAsync(id);

        if (payment is null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (!isAdmin && payment.UserId != userId)
            return Forbid();

        var normalizedStatus = request.Status.Trim();
        bool success;

        if (normalizedStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            success = await _paymentService.CompleteAsync(id, Request.Headers.Authorization.ToString());
        }
        else if (normalizedStatus.Equals("Failed", StringComparison.OrdinalIgnoreCase))
        {
            success = await _paymentService.FailAsync(id, Request.Headers.Authorization.ToString());
        }
        else
        {
            return BadRequest("Status must be either 'Completed' or 'Failed'.");
        }

        if (!success)
            return BadRequest("Payment status cannot be updated.");

        return NoContent();
    }

    private static void AttachLinks(PaymentResponseDto payment)
    {
        payment.Links =
        [
            new LinkDto { Rel = "self", Href = $"/api/payments/{payment.Id}", Method = "GET" },
            new LinkDto { Rel = "reservation", Href = $"/api/reservations/{payment.ReservationId}", Method = "GET" },
            new LinkDto { Rel = "notifications", Href = $"/api/notifications/user/{payment.UserId}", Method = "GET" }
        ];

        if (payment.Status == "Pending")
        {
            payment.Links.Add(new LinkDto { Rel = "update-status", Href = $"/api/payments/{payment.Id}", Method = "PATCH" });
        }
    }
}
