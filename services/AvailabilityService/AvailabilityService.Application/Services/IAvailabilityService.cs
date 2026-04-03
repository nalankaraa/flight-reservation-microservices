using AvailabilityService.Application.Dtos;

namespace AvailabilityService.Application.Services;

public interface IAvailabilityService
{
    Task<FlightAvailabilityDto> GetAvailabilityAsync(string flightId);
    Task<List<SeatAvailabilityDto>> GetSeatsAsync(string flightId);
    Task<SeatAvailabilityDto?> LockSeatAsync(string flightId, LockSeatRequestDto request, string userId);
    Task<SeatAvailabilityDto?> ConfirmSeatAsync(string flightId, string seatNumber, string userId);
    Task<bool> ReleaseSeatAsync(string flightId, ReleaseSeatRequestDto request, string userId, bool allowAnyUser);
}