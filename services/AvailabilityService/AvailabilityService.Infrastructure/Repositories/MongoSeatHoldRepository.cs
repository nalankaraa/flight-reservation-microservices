using AvailabilityService.Application.Repositories;
using AvailabilityService.Domain.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace AvailabilityService.Infrastructure.Repositories;

public class MongoSeatHoldRepository : ISeatHoldRepository
{
    private readonly IMongoCollection<SeatHoldDocument> _collection;

    public MongoSeatHoldRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<SeatHoldDocument>("seat-holds");
    }

    public async Task AddAsync(SeatHold hold)
    {
        await _collection.InsertOneAsync(MapToDocument(hold));
    }

    public async Task<SeatHold?> GetByIdAsync(string id)
    {
        var document = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return document is null ? null : MapToDomain(document);
    }

    public async Task UpdateAsync(SeatHold hold)
    {
        await _collection.ReplaceOneAsync(x => x.Id == hold.Id, MapToDocument(hold));
    }

    private static SeatHoldDocument MapToDocument(SeatHold hold)
    {
        return new SeatHoldDocument
        {
            Id = hold.Id,
            FlightId = hold.FlightId,
            UserId = hold.UserId,
            SeatCount = hold.SeatCount,
            ReservedUntilUtc = hold.ReservedUntilUtc,
            Status = hold.Status
        };
    }

    private static SeatHold MapToDomain(SeatHoldDocument document)
    {
        return new SeatHold
        {
            Id = document.Id,
            FlightId = document.FlightId,
            UserId = document.UserId,
            SeatCount = document.SeatCount,
            ReservedUntilUtc = document.ReservedUntilUtc,
            Status = document.Status
        };
    }

    private class SeatHoldDocument
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public string FlightId { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public int SeatCount { get; set; }
        public DateTime ReservedUntilUtc { get; set; }
        public string Status { get; set; } = default!;
    }
}
