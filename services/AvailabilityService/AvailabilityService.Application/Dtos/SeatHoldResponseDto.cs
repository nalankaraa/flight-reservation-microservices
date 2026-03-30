namespace AvailabilityService.Application.Dtos;

public class SeatHoldResponseDto
{
	public string Id { get; set; } = default!;
	public string FlightId { get; set; } = default!;
	public string UserId { get; set; } = default!;
	public int SeatCount { get; set; }
	public DateTime ReservedUntilUtc { get; set; }
	public string Status { get; set; } = default!;
}