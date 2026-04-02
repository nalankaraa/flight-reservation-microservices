using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Application.Dtos;

public class ReleaseSeatRequestDto
{
    [Required]
    public string SeatNumber { get; set; } = default!;
}