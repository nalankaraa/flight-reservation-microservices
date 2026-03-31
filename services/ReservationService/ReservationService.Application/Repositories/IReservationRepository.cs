using ReservationService.Domain.Entities;

namespace ReservationService.Application.Repositories;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation);
    Task<List<Reservation>> GetAllAsync();
}