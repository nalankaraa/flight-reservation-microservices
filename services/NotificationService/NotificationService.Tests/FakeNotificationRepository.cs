using NotificationService.Application.Repositories;
using NotificationService.Domain.Entities;

namespace NotificationService.Tests;

public class FakeNotificationRepository : INotificationRepository
{
    public List<Notification> Notifications { get; } = new();

    public Task AddAsync(Notification notification)
    {
        Notifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task<Notification?> GetByIdAsync(string id)
    {
        return Task.FromResult(Notifications.FirstOrDefault(x => x.Id == id));
    }

    public Task<List<Notification>> GetByUserIdAsync(string userId)
    {
        return Task.FromResult(Notifications.Where(x => x.UserId == userId).ToList());
    }

    public Task UpdateAsync(Notification notification)
    {
        var existing = Notifications.FirstOrDefault(x => x.Id == notification.Id);

        if (existing != null)
        {
            Notifications.Remove(existing);
            Notifications.Add(notification);
        }

        return Task.CompletedTask;
    }
}