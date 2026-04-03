using BuildingBlocks.Application.Hateoas;

namespace FlightService.Application.Dtos;

public class FlightResponseDto
{
    public string Id { get; set; } = default!;
    public string From { get; set; } = default!;
    public string To { get; set; } = default!;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeatCount { get; set; }
    public List<LinkDto> Links { get; set; } = [];
}
