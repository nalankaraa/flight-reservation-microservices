namespace ReservationService.Application.Dtos;

public class ReservationResponseDto
{
    public string Id { get; set; } = default!;
    public string FlightId { get; set; } = default!;
    public string PassengerName { get; set; } = default!;
    public string SeatNumber { get; set; } = default!;
}