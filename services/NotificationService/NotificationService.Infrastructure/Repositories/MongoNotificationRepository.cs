using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NotificationService.Application.Repositories;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Repositories;

public class MongoNotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<NotificationDocument> _collection;

    public MongoNotificationRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<NotificationDocument>("notifications");
    }

    public async Task AddAsync(Notification notification)
    {
        await _collection.InsertOneAsync(MapToDocument(notification));
    }

    public async Task<Notification?> GetByIdAsync(string id)
    {
        var document = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return document is null ? null : MapToDomain(document);
    }

    public async Task<List<Notification>> GetByUserIdAsync(string userId)
    {
        var documents = await _collection.Find(x => x.UserId == userId).ToListAsync();
        return documents.Select(MapToDomain).ToList();
    }

    public async Task UpdateAsync(Notification notification)
    {
        await _collection.ReplaceOneAsync(x => x.Id == notification.Id, MapToDocument(notification));
    }

    private static NotificationDocument MapToDocument(Notification notification)
    {
        return new NotificationDocument
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

    private static Notification MapToDomain(NotificationDocument document)
    {
        return new Notification
        {
            Id = document.Id,
            UserId = document.UserId,
            Title = document.Title,
            Message = document.Message,
            Type = document.Type,
            CreatedAtUtc = document.CreatedAtUtc,
            IsRead = document.IsRead,
            IsSent = document.IsSent
        };
    }

    private class NotificationDocument
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string Type { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
        public bool IsRead { get; set; }
        public bool IsSent { get; set; }
    }
}
