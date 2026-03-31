namespace PaymentService.Domain.Entities;

public class Payment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ReservationId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
}