using BuildingBlocks.Application.Hateoas;

namespace ReservationService.Application.Dtos;

public class ReservationResponseDto
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public string? FlightId { get; set; }
    public string? PassengerName { get; set; }
    public string? SeatNumber { get; set; }
    public string? Message { get; set; }
    public List<LinkDto> Links { get; set; } = [];
}
