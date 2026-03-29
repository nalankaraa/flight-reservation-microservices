using FlightService.Domain.Entities;

namespace FlightService.Application.Repositories;

public interface IFlightRepository
{
    Task AddAsync(Flight flight);
    Task<Flight?> GetByIdAsync(string id);
    Task<List<Flight>> GetAllAsync();
    Task UpdateAsync(Flight flight);
    Task DeleteAsync(string id);
}