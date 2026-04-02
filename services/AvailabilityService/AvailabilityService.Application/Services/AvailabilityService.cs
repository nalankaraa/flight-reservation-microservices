using AvailabilityService.Application.Dtos;
using AvailabilityService.Application.Repositories;
using AvailabilityService.Domain.Entities;

namespace AvailabilityService.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly ISeatHoldRepository _repository;

    public AvailabilityService(ISeatHoldRepository repository)
    {
        _repository = repository;
    }

    public async Task<FlightAvailabilityDto> GetAvailabilityAsync(string flightId)
    {
        var seats = await GetSeatAvailabilityInternalAsync(flightId);

        return new FlightAvailabilityDto
        {
            FlightId = flightId,
            TotalTrackedSeats = seats.Count,
            AvailableSeats = seats.Count(x => x.IsAvailable),
            LockedSeats = seats.Count(x => !x.IsAvailable)
        };
    }

    public Task<List<SeatAvailabilityDto>> GetSeatsAsync(string flightId)
    {
        return GetSeatAvailabilityInternalAsync(flightId);
    }

    public async Task<SeatAvailabilityDto?> LockSeatAsync(string flightId, LockSeatRequestDto request, string userId)
    {
        var nowUtc = DateTime.UtcNow;
        var hold = new SeatHold
        {
            Id = BuildId(flightId, request.SeatNumber),
            FlightId = flightId,
            SeatNumber = NormalizeSeatNumber(request.SeatNumber),
            UserId = userId,
            ReservedUntilUtc = nowUtc.AddMinutes(request.HoldMinutes),
            Status = "Locked",
            LastUpdatedUtc = nowUtc
        };

        var locked = await _repository.TryLockSeatAsync(hold, nowUtc);
        return locked is null ? null : MapToSeatAvailability(locked, nowUtc);
    }

    public async Task<SeatAvailabilityDto?> ConfirmSeatAsync(string flightId, string seatNumber, string userId)
    {
        if (string.IsNullOrWhiteSpace(seatNumber))
            return null;

        var nowUtc = DateTime.UtcNow;
        var confirmed = await _repository.ConfirmSeatAsync(flightId, seatNumber, userId, nowUtc);
        return confirmed is null ? null : MapToSeatAvailability(confirmed, nowUtc);
    }

    public Task<bool> ReleaseSeatAsync(string flightId, ReleaseSeatRequestDto request, string userId, bool allowAnyUser)
    {
        return _repository.ReleaseSeatAsync(
            flightId,
            request.SeatNumber,
            userId,
            allowAnyUser,
            DateTime.UtcNow);
    }

    private async Task<List<SeatAvailabilityDto>> GetSeatAvailabilityInternalAsync(string flightId)
    {
        var nowUtc = DateTime.UtcNow;
        var seats = await _repository.GetByFlightIdAsync(flightId);
        var result = new List<SeatAvailabilityDto>(seats.Count);

        foreach (var seat in seats)
        {
            if (IsExpired(seat, nowUtc))
            {
                seat.Status = "Expired";
                seat.LastUpdatedUtc = nowUtc;
                await _repository.UpdateAsync(seat);
            }

            result.Add(MapToSeatAvailability(seat, nowUtc));
        }

        return result
            .OrderBy(x => x.SeatNumber, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsExpired(SeatHold hold, DateTime nowUtc)
    {
        return hold.Status == "Locked" && hold.ReservedUntilUtc <= nowUtc;
    }

    private static SeatAvailabilityDto MapToSeatAvailability(SeatHold hold, DateTime nowUtc)
    {
        var isAvailable =
            hold.Status == "Expired" ||
            hold.Status == "Released" ||
            (hold.Status == "Locked" && hold.ReservedUntilUtc <= nowUtc);

        return new SeatAvailabilityDto
        {
            FlightId = hold.FlightId,
            SeatNumber = hold.SeatNumber,
            UserId = isAvailable ? null : hold.UserId,
            IsAvailable = isAvailable,
            ReservedUntilUtc = hold.Status == "Reserved" || isAvailable ? null : hold.ReservedUntilUtc,
            Status = isAvailable ? "Available" : hold.Status
        };
    }

    private static string NormalizeSeatNumber(string seatNumber)
    {
        return seatNumber.Trim().ToUpperInvariant();
    }

    private static string BuildId(string flightId, string seatNumber)
    {
        return $"{flightId.Trim()}::{NormalizeSeatNumber(seatNumber)}";
    }
}