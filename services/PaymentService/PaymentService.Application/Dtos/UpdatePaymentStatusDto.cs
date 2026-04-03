using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.Dtos;

public class UpdatePaymentStatusDto
{
    [Required]
    public string Status { get; set; } = default!;
}
