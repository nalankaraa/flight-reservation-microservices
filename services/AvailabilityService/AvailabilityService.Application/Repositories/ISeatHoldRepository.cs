using AvailabilityService.Domain.Entities;

namespace AvailabilityService.Application.Repositories;

public interface ISeatHoldRepository
{
    Task<SeatHold?> TryLockSeatAsync(SeatHold hold, DateTime nowUtc);
    Task<SeatHold?> GetByFlightAndSeatAsync(string flightId, string seatNumber);
    Task<List<SeatHold>> GetByFlightIdAsync(string flightId);
    Task<SeatHold?> ConfirmSeatAsync(string flightId, string seatNumber, string userId, DateTime nowUtc);
    Task<bool> ReleaseSeatAsync(string flightId, string seatNumber, string? userId, bool allowAnyUser, DateTime nowUtc);
    Task UpdateAsync(SeatHold hold);
}