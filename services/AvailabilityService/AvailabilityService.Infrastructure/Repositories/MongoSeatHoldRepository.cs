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

    public async Task<SeatHold?> TryLockSeatAsync(SeatHold hold, DateTime nowUtc)
    {
        var document = MapToDocument(hold);
        var filter = Builders<SeatHoldDocument>.Filter.And(
            Builders<SeatHoldDocument>.Filter.Eq(x => x.Id, hold.Id),
            Builders<SeatHoldDocument>.Filter.Or(
                Builders<SeatHoldDocument>.Filter.Ne(x => x.Status, "Locked"),
                Builders<SeatHoldDocument>.Filter.Lte(x => x.ReservedUntilUtc, nowUtc),
                Builders<SeatHoldDocument>.Filter.Eq(x => x.UserId, hold.UserId)));

        try
        {
            var updated = await _collection.FindOneAndReplaceAsync(
                filter,
                document,
                new FindOneAndReplaceOptions<SeatHoldDocument>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                });

            return updated is null ? null : MapToDomain(updated);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return null;
        }
    }

    public async Task<SeatHold?> GetByFlightAndSeatAsync(string flightId, string seatNumber)
    {
        var normalizedSeatNumber = seatNumber.Trim().ToUpperInvariant();
        var document = await _collection
            .Find(x => x.FlightId == flightId && x.SeatNumber == normalizedSeatNumber)
            .FirstOrDefaultAsync();

        return document is null ? null : MapToDomain(document);
    }

    public async Task<List<SeatHold>> GetByFlightIdAsync(string flightId)
    {
        var documents = await _collection.Find(x => x.FlightId == flightId).ToListAsync();
        return documents.Select(MapToDomain).ToList();
    }

    public async Task<SeatHold?> ConfirmSeatAsync(string flightId, string seatNumber, string userId, DateTime nowUtc)
    {
        var normalizedSeatNumber = seatNumber.Trim().ToUpperInvariant();
        var filter = Builders<SeatHoldDocument>.Filter.And(
            Builders<SeatHoldDocument>.Filter.Eq(x => x.FlightId, flightId),
            Builders<SeatHoldDocument>.Filter.Eq(x => x.SeatNumber, normalizedSeatNumber),
            Builders<SeatHoldDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<SeatHoldDocument>.Filter.Eq(x => x.Status, "Locked"),
            Builders<SeatHoldDocument>.Filter.Gt(x => x.ReservedUntilUtc, nowUtc));

        var update = Builders<SeatHoldDocument>.Update
            .Set(x => x.Status, "Reserved")
            .Set(x => x.LastUpdatedUtc, nowUtc);

        var result = await _collection.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<SeatHoldDocument>
            {
                ReturnDocument = ReturnDocument.After
            });

        return result is null ? null : MapToDomain(result);
    }

    public async Task<bool> ReleaseSeatAsync(string flightId, string seatNumber, string? userId, bool allowAnyUser, DateTime nowUtc)
    {
        var normalizedSeatNumber = seatNumber.Trim().ToUpperInvariant();
        var filter = Builders<SeatHoldDocument>.Filter.And(
            Builders<SeatHoldDocument>.Filter.Eq(x => x.FlightId, flightId),
            Builders<SeatHoldDocument>.Filter.Eq(x => x.SeatNumber, normalizedSeatNumber),
            Builders<SeatHoldDocument>.Filter.Or(
                Builders<SeatHoldDocument>.Filter.And(
                    Builders<SeatHoldDocument>.Filter.Eq(x => x.Status, "Locked"),
                    Builders<SeatHoldDocument>.Filter.Gt(x => x.ReservedUntilUtc, nowUtc)),
                Builders<SeatHoldDocument>.Filter.Eq(x => x.Status, "Reserved")));

        if (!allowAnyUser)
        {
            filter &= Builders<SeatHoldDocument>.Filter.Eq(x => x.UserId, userId);
        }

        var update = Builders<SeatHoldDocument>.Update
            .Set(x => x.Status, "Released")
            .Set(x => x.ReservedUntilUtc, nowUtc)
            .Set(x => x.LastUpdatedUtc, nowUtc);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount == 1;
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
            SeatNumber = hold.SeatNumber,
            UserId = hold.UserId,
            ReservedUntilUtc = hold.ReservedUntilUtc,
            Status = hold.Status,
            LastUpdatedUtc = hold.LastUpdatedUtc
        };
    }

    private static SeatHold MapToDomain(SeatHoldDocument document)
    {
        return new SeatHold
        {
            Id = document.Id,
            FlightId = document.FlightId,
            SeatNumber = document.SeatNumber,
            UserId = document.UserId,
            ReservedUntilUtc = document.ReservedUntilUtc,
            Status = document.Status,
            LastUpdatedUtc = document.LastUpdatedUtc
        };
    }

    private class SeatHoldDocument
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public string FlightId { get; set; } = default!;
        public string SeatNumber { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public DateTime ReservedUntilUtc { get; set; }
        public string Status { get; set; } = default!;
        public DateTime LastUpdatedUtc { get; set; }
    }
}