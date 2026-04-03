using ReservationService.Application.Repositories;
using ReservationService.Domain.Entities;

namespace ReservationService.Infrastructure.Repositories;

public class InMemoryReservationRepository : IReservationRepository
{
    private readonly List<Reservation> _reservations = new();

    public Task AddAsync(Reservation reservation)
    {
        _reservations.Add(reservation);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string reservationId)
    {
        var existing = _reservations.FirstOrDefault(x => x.Id == reservationId);

        if (existing is not null)
        {
            _reservations.Remove(existing);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsByFlightAndSeatAsync(string flightId, string seatNumber)
    {
        return Task.FromResult(_reservations.Any(x =>
            x.FlightId == flightId &&
            x.SeatNumber.Equals(seatNumber, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<List<Reservation>> GetAllAsync()
    {
        return Task.FromResult(_reservations.ToList());
    }

    public Task<Reservation?> GetByIdAsync(string id)
    {
        return Task.FromResult(_reservations.FirstOrDefault(x => x.Id == id));
    }

    public Task<List<Reservation>> GetByUserIdAsync(string userId)
    {
        return Task.FromResult(_reservations.Where(x => x.UserId == userId).ToList());
    }
}
