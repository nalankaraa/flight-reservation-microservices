namespace ReservationService.Application.Clients;

public class FlightLookupResult
{
    public bool Success { get; set; }
    public bool IsNotFound { get; set; }
    public bool IsServiceUnavailable { get; set; }
    public decimal Price { get; set; }
}
