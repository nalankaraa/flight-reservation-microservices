using NotificationService.Domain.Entities;

namespace NotificationService.Application.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<Notification?> GetByIdAsync(string id);
    Task<List<Notification>> GetByUserIdAsync(string userId);
    Task UpdateAsync(Notification notification);
}