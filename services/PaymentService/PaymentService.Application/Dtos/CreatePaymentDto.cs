using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.Dtos;

public class CreatePaymentDto
{
    [Required]
    public string ReservationId { get; set; } = default!;

    [Range(1, int.MaxValue)]
    public decimal Amount { get; set; }
}