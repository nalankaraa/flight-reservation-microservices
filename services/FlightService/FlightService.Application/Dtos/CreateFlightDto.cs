using System.ComponentModel.DataAnnotations;

namespace FlightService.Application.Dtos;

public class CreateFlightDto
{
    [Required]
    public string From { get; set; } = default!;

    [Required]
    public string To { get; set; } = default!;

    [Required]
    public DateTime DepartureTime { get; set; }

    [Required]
    public DateTime ArrivalTime { get; set; }

    [Range(1, int.MaxValue)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int AvailableSeatCount { get; set; }
}