using FlightService.Application.Repositories;
using FlightService.Domain.Entities;

namespace FlightService.Infrastructure.Repositories;

public class InMemoryFlightRepository : IFlightRepository
{
    private readonly List<Flight> _flights = new();

    public Task AddAsync(Flight flight)
    {
        _flights.Add(flight);
        return Task.CompletedTask;
    }

    public Task<Flight?> GetByIdAsync(string id)
    {
        var flight = _flights.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(flight);
    }

    public Task<List<Flight>> GetAllAsync()
    {
        return Task.FromResult(_flights.ToList());
    }
}