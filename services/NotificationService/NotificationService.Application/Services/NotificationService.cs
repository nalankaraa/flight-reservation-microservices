using NotificationService.Application.Dtos;
using NotificationService.Application.Repositories;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<NotificationResponseDto> CreateAsync(CreateNotificationDto request)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            CreatedAtUtc = DateTime.UtcNow,
            IsRead = false,
            IsSent = false
        };

        await _repository.AddAsync(notification);

        return MapToDto(notification);
    }

    public async Task<NotificationResponseDto?> GetByIdAsync(string id)
    {
        var notification = await _repository.GetByIdAsync(id);

        if (notification is null)
            return null;

        return MapToDto(notification);
    }

    public async Task<List<NotificationResponseDto>> GetByUserIdAsync(string userId)
    {
        var notifications = await _repository.GetByUserIdAsync(userId);
        return notifications
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<bool> SendAsync(string id)
    {
        var notification = await _repository.GetByIdAsync(id);

        if (notification is null)
            return false;

        if (notification.IsSent)
            return false;

        notification.IsSent = true;
        await _repository.UpdateAsync(notification);

        return true;
    }

    public async Task<bool> MarkAsReadAsync(string id)
    {
        var notification = await _repository.GetByIdAsync(id);

        if (notification is null)
            return false;

        if (notification.IsRead)
            return false;

        notification.IsRead = true;
        await _repository.UpdateAsync(notification);

        return true;
    }

    private static NotificationResponseDto MapToDto(Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            CreatedAtUtc = notification.CreatedAtUtc,
            IsRead = notification.IsRead,
            IsSent = notification.IsSent
        };
    }
}