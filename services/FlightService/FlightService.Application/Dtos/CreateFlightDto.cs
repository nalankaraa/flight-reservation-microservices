namespace FlightService.Application.Dtos;

public class CreateFlightDto
{
    public string From { get; set; } = default!;
    public string To { get; set; } = default!;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeatCount { get; set; }
}