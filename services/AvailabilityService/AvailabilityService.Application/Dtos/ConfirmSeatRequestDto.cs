using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Application.Dtos;

public class ConfirmSeatRequestDto
{
    [Required]
    public string SeatNumber { get; set; } = default!;
}