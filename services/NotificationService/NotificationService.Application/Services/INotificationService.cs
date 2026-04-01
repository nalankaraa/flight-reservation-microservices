using NotificationService.Application.Dtos;

namespace NotificationService.Application.Services;

public interface INotificationService
{
    Task<NotificationResponseDto> CreateAsync(CreateNotificationDto request);
    Task<NotificationResponseDto?> GetByIdAsync(string id);
    Task<List<NotificationResponseDto>> GetByUserIdAsync(string userId);
    Task<bool> SendAsync(string id);
}