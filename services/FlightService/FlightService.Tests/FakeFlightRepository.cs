using FlightService.Application.Repositories;
using FlightService.Domain.Entities;

namespace FlightService.Tests;

public class FakeFlightRepository : IFlightRepository
{
    public List<Flight> Flights { get; } = new();

    public Task AddAsync(Flight flight)
    {
        Flights.Add(flight);
        return Task.CompletedTask;
    }

    public Task<Flight?> GetByIdAsync(string id)
    {
        return Task.FromResult(Flights.FirstOrDefault(x => x.Id == id));
    }

    public Task<List<Flight>> GetAllAsync()
    {
        return Task.FromResult(Flights.ToList());
    }
}