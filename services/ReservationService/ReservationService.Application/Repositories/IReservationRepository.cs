using ReservationService.Domain.Entities;

namespace ReservationService.Application.Repositories;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation);
    Task DeleteAsync(string reservationId);
    Task<bool> ExistsByFlightAndSeatAsync(string flightId, string seatNumber);
    Task<List<Reservation>> GetAllAsync();
    Task<List<Reservation>> GetByUserIdAsync(string userId);
}