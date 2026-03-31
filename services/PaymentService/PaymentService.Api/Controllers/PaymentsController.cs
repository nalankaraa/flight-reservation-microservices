using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Dtos;
using PaymentService.Application.Services;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentDto request)
    {
        var result = await _paymentService.CreateAsync(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _paymentService.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(string id)
    {
        var success = await _paymentService.CompleteAsync(id);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/fail")]
    public async Task<IActionResult> Fail(string id)
    {
        var success = await _paymentService.FailAsync(id);

        if (!success)
            return NotFound();

        return NoContent();
    }
}