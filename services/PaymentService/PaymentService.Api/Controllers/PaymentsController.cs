using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentService.Application.Dtos;
using PaymentService.Application.Services;
using BuildingBlocks.Application.Hateoas;

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

        AttachLinks(result);
        return Ok(result);
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Complete(string id)
    {
        var success = await _paymentService.CompleteAsync(id);

        if (!success)
            return BadRequest("Payment cannot be completed.");

        return NoContent();
    }

    [HttpPost("{id}/fail")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Fail(string id)
    {
        var success = await _paymentService.FailAsync(id);

        if (!success)
            return BadRequest("Payment cannot be failed.");

        return NoContent();
    }

    private static void AttachLinks(PaymentResponseDto payment)
    {
        payment.Links =
        [
            new LinkDto { Rel = "self", Href = $"/api/payments/{payment.Id}", Method = "GET" },
            new LinkDto { Rel = "reservation", Href = $"/api/reservations/{payment.ReservationId}", Method = "GET" }
        ];

        if (payment.Status == "Pending")
        {
            payment.Links.Add(new LinkDto { Rel = "complete", Href = $"/api/payments/{payment.Id}/complete", Method = "POST" });
            payment.Links.Add(new LinkDto { Rel = "fail", Href = $"/api/payments/{payment.Id}/fail", Method = "POST" });
        }
    }
}
