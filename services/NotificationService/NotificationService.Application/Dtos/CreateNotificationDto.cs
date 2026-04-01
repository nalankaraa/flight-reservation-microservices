using System.ComponentModel.DataAnnotations;

namespace NotificationService.Application.Dtos;

public class CreateNotificationDto
{
    [Required]
    public string UserId { get; set; } = default!;

    [Required]
    public string Title { get; set; } = default!;

    [Required]
    public string Message { get; set; } = default!;

    [Required]
    public string Type { get; set; } = default!;
}