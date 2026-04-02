using AuthService.Application.Repositories;
using AuthService.Domain.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace AuthService.Infrastructure.Repositories;

public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<UserDocument> _collection;

    public MongoUserRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<UserDocument>("users");

        var emailIndex = Builders<UserDocument>.IndexKeys.Ascending(x => x.Email);
        _collection.Indexes.CreateOne(new CreateIndexModel<UserDocument>(emailIndex, new CreateIndexOptions
        {
            Unique = true
        }));
    }

    public async Task AddAsync(User user)
    {
        await _collection.InsertOneAsync(MapToDocument(user));
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var document = await _collection.Find(x => x.Email == normalizedEmail).FirstOrDefaultAsync();
        return document is null ? null : MapToDomain(document);
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        var document = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return document is null ? null : MapToDomain(document);
    }

    private static UserDocument MapToDocument(User user)
    {
        return new UserDocument
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }

    private static User MapToDomain(UserDocument document)
    {
        return new User
        {
            Id = document.Id,
            Email = document.Email,
            PasswordHash = document.PasswordHash,
            Role = document.Role,
            CreatedAtUtc = document.CreatedAtUtc
        };
    }

    private class UserDocument
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
    }
}