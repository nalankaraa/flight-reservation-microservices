using MongoDB.Driver;
using ReservationService.Application.Exceptions;
using ReservationService.Application.Repositories;
using ReservationService.Domain.Entities;

namespace ReservationService.Infrastructure.Repositories;

public class MongoReservationRepository : IReservationRepository
{
    private readonly IMongoCollection<Reservation> _reservations;

    public MongoReservationRepository(IMongoDatabase database)
    {
        _reservations = database.GetCollection<Reservation>("reservations");

        var indexKeys = Builders<Reservation>.IndexKeys
            .Ascending(x => x.FlightId)
            .Ascending(x => x.SeatNumber);

        var indexModel = new CreateIndexModel<Reservation>(
            indexKeys,
            new CreateIndexOptions
            {
                Unique = true,
                Name = "ux_flight_seat"
            });

        _reservations.Indexes.CreateOne(indexModel);
    }

    public async Task AddAsync(Reservation reservation)
    {
        NormalizeReservation(reservation);

        try
        {
            await _reservations.InsertOneAsync(reservation);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new DuplicateSeatReservationException("Duplicate seat");
        }
    }

    public async Task DeleteAsync(string reservationId)
    {
        await _reservations.DeleteOneAsync(x => x.Id == reservationId);
    }

    public async Task<bool> ExistsByFlightAndSeatAsync(string flightId, string seatNumber)
    {
        var normalizedSeatNumber = NormalizeSeatNumber(seatNumber);

        return await _reservations.Find(x =>
            x.FlightId == flightId &&
            x.SeatNumber == normalizedSeatNumber).AnyAsync();
    }

    public async Task<List<Reservation>> GetAllAsync()
    {
        return await _reservations.Find(_ => true).ToListAsync();
    }

    public async Task<Reservation?> GetByIdAsync(string id)
    {
        return await _reservations.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Reservation>> GetByUserIdAsync(string userId)
    {
        return await _reservations.Find(x => x.UserId == userId).ToListAsync();
    }

    private static void NormalizeReservation(Reservation reservation)
    {
        reservation.SeatNumber = NormalizeSeatNumber(reservation.SeatNumber);
    }

    private static string NormalizeSeatNumber(string seatNumber)
    {
        return seatNumber.Trim().ToUpperInvariant();
    }
}
