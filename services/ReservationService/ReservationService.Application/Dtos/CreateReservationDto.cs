using System.ComponentModel.DataAnnotations;

namespace ReservationService.Application.Dtos;

public class CreateReservationDto
{
    [Required]
    public string FlightId { get; set; } = default!;

    [Required]
    public string PassengerName { get; set; } = default!;

    [Required]
    public string SeatNumber { get; set; } = default!;
}