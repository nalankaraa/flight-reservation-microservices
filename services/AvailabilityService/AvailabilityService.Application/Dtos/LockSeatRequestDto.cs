using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Application.Dtos;

public class LockSeatRequestDto
{
    [Required]
    public string SeatNumber { get; set; } = default!;

    [Range(1, 60)]
    public int HoldMinutes { get; set; } = 10;
}