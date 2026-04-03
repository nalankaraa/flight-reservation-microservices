using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ReservationService.Application.Exceptions;
using ReservationService.Application.Repositories;
using ReservationService.Domain.Entities;

namespace ReservationService.Infrastructure.Repositories;

public class MongoReservationRepository : IReservationRepository
{
    private readonly IMongoCollection<ReservationDocument> _collection;

    public MongoReservationRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ReservationDocument>("reservations");

        var indexKeys = Builders<ReservationDocument>.IndexKeys
            .Ascending(x => x.FlightId)
            .Ascending(x => x.SeatNumber);

        _collection.Indexes.CreateOne(new CreateIndexModel<ReservationDocument>(
            indexKeys,
            new CreateIndexOptions { Unique = true }));
    }

    public async Task AddAsync(Reservation reservation)
    {
        try
        {
            await _collection.InsertOneAsync(MapToDocument(reservation));
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new DuplicateSeatReservationException("Duplicate seat");
        }
    }

    public async Task DeleteAsync(string reservationId)
    {
        await _collection.DeleteOneAsync(x => x.Id == reservationId);
    }

    public async Task<bool> ExistsByFlightAndSeatAsync(string flightId, string seatNumber)
    {
        return await _collection.Find(x =>
            x.FlightId == flightId &&
            x.SeatNumber == seatNumber).AnyAsync();
    }

    public async Task<List<Reservation>> GetAllAsync()
    {
        var docs = await _collection.Find(_ => true).ToListAsync();
        return docs.Select(MapToDomain).ToList();
    }

    public async Task<Reservation?> GetByIdAsync(string id)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (doc is null)
            return null;

        return MapToDomain(doc);
    }

    public async Task<List<Reservation>> GetByUserIdAsync(string userId)
    {
        var docs = await _collection.Find(x => x.UserId == userId).ToListAsync();
        return docs.Select(MapToDomain).ToList();
    }

    private static ReservationDocument MapToDocument(Reservation reservation)
    {
        return new ReservationDocument
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            FlightId = reservation.FlightId,
            PassengerName = reservation.PassengerName,
            SeatNumber = reservation.SeatNumber
        };
    }

    private static Reservation MapToDomain(ReservationDocument doc)
    {
        return new Reservation
        {
            Id = doc.Id,
            UserId = doc.UserId,
            FlightId = doc.FlightId,
            PassengerName = doc.PassengerName,
            SeatNumber = doc.SeatNumber
        };
    }

    private class ReservationDocument
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string FlightId { get; set; } = default!;
        public string PassengerName { get; set; } = default!;
        public string SeatNumber { get; set; } = default!;
    }
}