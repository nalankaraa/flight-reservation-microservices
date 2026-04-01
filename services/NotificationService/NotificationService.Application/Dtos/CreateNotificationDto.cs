namespace NotificationService.Application.Dtos;

public class CreateNotificationDto
{
	public string UserId { get; set; } = default!;
	public string Title { get; set; } = default!;
	public string Message { get; set; } = default!;
	public string Type { get; set; } = default!;
}