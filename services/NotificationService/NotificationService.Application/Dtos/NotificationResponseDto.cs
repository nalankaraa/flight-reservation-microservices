using BuildingBlocks.Application.Hateoas;

namespace NotificationService.Application.Dtos;

public class NotificationResponseDto
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Type { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRead { get; set; }
    public bool IsSent { get; set; }
    public List<LinkDto> Links { get; set; } = [];
}
