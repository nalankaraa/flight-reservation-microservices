using ReservationService.Application.Repositories;
using ReservationService.Domain.Entities;

namespace ReservationService.Tests;

public class FakeReservationRepository : IReservationRepository
{
    private readonly List<Reservation> _reservations = new();

    public Task AddAsync(Reservation reservation)
    {
        _reservations.Add(reservation);
        return Task.CompletedTask;
    }

    public Task<List<Reservation>> GetAllAsync()
    {
        return Task.FromResult(_reservations);
    }
}