using FlightService.Application.Dtos;

namespace FlightService.Application.Services;

public interface IFlightService
{
    Task<FlightResponseDto> CreateAsync(CreateFlightDto request);
    Task<FlightResponseDto?> GetByIdAsync(string id);
    Task<List<FlightResponseDto>> GetAllAsync();
    Task<bool> UpdateAsync(string id, UpdateFlightDto request);
    Task<bool> DeleteAsync(string id);
}