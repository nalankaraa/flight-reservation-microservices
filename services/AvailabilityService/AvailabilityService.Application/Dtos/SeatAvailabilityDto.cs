namespace AvailabilityService.Application.Dtos;

public class SeatAvailabilityDto
{
    public string FlightId { get; set; } = default!;
    public string SeatNumber { get; set; } = default!;
    public string? UserId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? ReservedUntilUtc { get; set; }
    public string Status { get; set; } = default!;
}