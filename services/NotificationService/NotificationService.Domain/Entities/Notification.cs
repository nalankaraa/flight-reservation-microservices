namespace NotificationService.Domain.Entities;

public class Notification
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string UserId { get; set; } = default!;
	public string Title { get; set; } = default!;
	public string Message { get; set; } = default!;
	public string Type { get; set; } = default!;
	public DateTime CreatedAtUtc { get; set; }
	public bool IsRead { get; set; }
	public bool IsSent { get; set; }
}