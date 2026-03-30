namespace AvailabilityService.Domain.Entities;

public class SeatHold
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FlightId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public int SeatCount { get; set; }
    public DateTime ReservedUntilUtc { get; set; }
    public string Status { get; set; } = default!;
}