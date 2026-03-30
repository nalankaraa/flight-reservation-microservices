namespace AvailabilityService.Application.Dtos;

public class CreateSeatHoldDto
{
    public string FlightId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public int SeatCount { get; set; }
    public int HoldMinutes { get; set; }
}