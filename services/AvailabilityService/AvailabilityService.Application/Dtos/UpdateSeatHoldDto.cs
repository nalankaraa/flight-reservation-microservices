using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Application.Dtos;

public class UpdateSeatHoldDto
{
    [Range(1, 60)]
    public int HoldMinutes { get; set; } = 10;
}
