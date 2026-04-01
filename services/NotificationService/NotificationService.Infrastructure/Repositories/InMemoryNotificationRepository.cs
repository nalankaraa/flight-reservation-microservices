using NotificationService.Application.Repositories;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Repositories;

public class InMemoryNotificationRepository : INotificationRepository
{
    private readonly List<Notification> _notifications = new();

    public Task AddAsync(Notification notification)
    {
        _notifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task<Notification?> GetByIdAsync(string id)
    {
        var notification = _notifications.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(notification);
    }

    public Task<List<Notification>> GetByUserIdAsync(string userId)
    {
        var notifications = _notifications
            .Where(x => x.UserId == userId)
            .ToList();

        return Task.FromResult(notifications);
    }

    public Task UpdateAsync(Notification notification)
    {
        var existing = _notifications.FirstOrDefault(x => x.Id == notification.Id);

        if (existing is not null)
        {
            existing.UserId = notification.UserId;
            existing.Title = notification.Title;
            existing.Message = notification.Message;
            existing.Type = notification.Type;
            existing.CreatedAtUtc = notification.CreatedAtUtc;
            existing.IsRead = notification.IsRead;
            existing.IsSent = notification.IsSent;
        }

        return Task.CompletedTask;
    }
}