using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Application.Dtos;

public class CreateSeatHoldDto
{
    [Required]
    public string FlightId { get; set; } = default!;

    [Required]
    public string UserId { get; set; } = default!;

    [Range(1, int.MaxValue)]
    public int SeatCount { get; set; }

    [Range(1, 60)]
    public int HoldMinutes { get; set; }
}