namespace AvailabilityService.Application.Dtos;

public class FlightAvailabilityDto
{
    public string FlightId { get; set; } = default!;
    public int TotalTrackedSeats { get; set; }
    public int AvailableSeats { get; set; }
    public int LockedSeats { get; set; }
}