using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentService.Application.Dtos;
using PaymentService.Application.Services;

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

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _paymentService.GetByIdAsync(id);

        if (result is null)
            return NotFound();

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
}
