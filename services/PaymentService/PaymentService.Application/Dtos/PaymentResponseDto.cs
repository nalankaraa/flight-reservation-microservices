using BuildingBlocks.Application.Hateoas;

namespace PaymentService.Application.Dtos;

public class PaymentResponseDto
{
    public string Id { get; set; } = default!;
    public string ReservationId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public List<LinkDto> Links { get; set; } = [];
}
