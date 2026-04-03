namespace ReservationService.Application.Clients;

public class PaymentCreationResult
{
    public bool Success { get; set; }
    public bool IsServiceUnavailable { get; set; }
    public string? PaymentId { get; set; }
    public string? Status { get; set; }
}
