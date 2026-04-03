namespace AvailabilityService.Domain.Entities;

public class SeatHold
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FlightId { get; set; } = default!;
    public string SeatNumber { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public DateTime ReservedUntilUtc { get; set; }
    public string Status { get; set; } = default!;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}