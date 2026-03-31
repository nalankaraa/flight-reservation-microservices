namespace PaymentService.Application.Dtos;

public class CreatePaymentDto
{
    public string ReservationId { get; set; } = default!;
    public decimal Amount { get; set; }
}