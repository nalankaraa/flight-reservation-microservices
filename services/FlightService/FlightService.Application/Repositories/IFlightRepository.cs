using FlightService.Domain.Entities;

namespace FlightService.Application.Repositories;

public interface IFlightRepository
{
    Task AddAsync(Flight flight);
    Task<Flight?> GetByIdAsync(string id);
    Task<List<Flight>> GetAllAsync();
}