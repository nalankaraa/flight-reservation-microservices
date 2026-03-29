using FlightService.Application.Dtos;
using FlightService.Application.Repositories;
using FlightService.Domain.Entities;

namespace FlightService.Application.Services;

public class FlightService : IFlightService
{
    private readonly IFlightRepository _repository;

    public FlightService(IFlightRepository repository)
    {
        _repository = repository;
    }

    public async Task<FlightResponseDto> CreateAsync(CreateFlightDto request)
    {
        var flight = new Flight
        {
            From = request.From,
            To = request.To,
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.ArrivalTime,
            Price = request.Price,
            AvailableSeatCount = request.AvailableSeatCount
        };

        await _repository.AddAsync(flight);

        return MapToDto(flight);
    }

    public async Task<FlightResponseDto?> GetByIdAsync(string id)
    {
        var flight = await _repository.GetByIdAsync(id);

        if (flight is null)
            return null;

        return MapToDto(flight);
    }

    public async Task<List<FlightResponseDto>> GetAllAsync()
    {
        var flights = await _repository.GetAllAsync();
        return flights.Select(MapToDto).ToList();
    }

    private static FlightResponseDto MapToDto(Flight flight)
    {
        return new FlightResponseDto
        {
            Id = flight.Id,
            From = flight.From,
            To = flight.To,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            Price = flight.Price,
            AvailableSeatCount = flight.AvailableSeatCount
        };
    }
}