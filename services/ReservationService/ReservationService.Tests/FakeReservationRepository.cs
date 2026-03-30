using ReservationService.Application.Repositories;
using ReservationService.Domain.Entities;

namespace ReservationService.Tests;

public class FakeReservationRepository : IReservationRepository
{
    public List<Reservation> Reservations { get; } = new();

    public Task AddAsync(Reservation reservation)
    {
        Reservations.Add(reservation);
        return Task.CompletedTask;
    }

    public Task<List<Reservation>> GetAllAsync()
    {
        return Task.FromResult(Reservations.ToList());
    }
}