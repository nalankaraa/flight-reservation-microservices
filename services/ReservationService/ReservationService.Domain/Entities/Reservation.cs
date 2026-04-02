namespace ReservationService.Domain.Entities;

public class Reservation
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string UserId { get; set; } = default!;
	public string FlightId { get; set; } = default!;
	public string PassengerName { get; set; } = default!;
	public string SeatNumber { get; set; } = default!;
}
