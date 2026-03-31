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

	public Task<List<Reservation>> GetAllAsync()
	{
		return Task.FromResult(_reservations.ToList());
	}
}