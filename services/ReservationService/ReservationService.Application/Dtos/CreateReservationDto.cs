namespace ReservationService.Application.Dtos;

public class CreateReservationDto
{
    public string FlightId { get; set; } = default!;
    public string PassengerName { get; set; } = default!;
    public string SeatNumber { get; set; } = default!;
}