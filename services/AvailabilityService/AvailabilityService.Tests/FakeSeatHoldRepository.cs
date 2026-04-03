using AvailabilityService.Application.Repositories;
using AvailabilityService.Domain.Entities;

namespace AvailabilityService.Tests;

public class FakeSeatHoldRepository : ISeatHoldRepository
{
    public List<SeatHold> Holds { get; } = new();

    public Task<SeatHold?> TryLockSeatAsync(SeatHold hold, DateTime nowUtc)
    {
        var existing = Holds.FirstOrDefault(x => x.Id == hold.Id);

        if (existing is null)
        {
            Holds.Add(Clone(hold));
            return Task.FromResult<SeatHold?>(Clone(hold));
        }

        var canReuseSeat =
            existing.Status != "Locked" ||
            existing.ReservedUntilUtc <= nowUtc ||
            string.Equals(existing.UserId, hold.UserId, StringComparison.Ordinal);

        if (!canReuseSeat)
            return Task.FromResult<SeatHold?>(null);

        Holds.Remove(existing);
        Holds.Add(Clone(hold));
        return Task.FromResult<SeatHold?>(Clone(hold));
    }

    public Task<SeatHold?> GetByFlightAndSeatAsync(string flightId, string seatNumber)
    {
        var hold = Holds.FirstOrDefault(x =>
            x.FlightId == flightId &&
            x.SeatNumber.Equals(seatNumber, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(hold is null ? null : Clone(hold));
    }

    public Task<List<SeatHold>> GetByFlightIdAsync(string flightId)
    {
        var holds = Holds
            .Where(x => x.FlightId == flightId)
            .Select(Clone)
            .ToList();

        return Task.FromResult(holds);
    }

    public Task<SeatHold?> ConfirmSeatAsync(string flightId, string seatNumber, string userId, DateTime nowUtc)
    {
        var hold = Holds.FirstOrDefault(x =>
            x.FlightId == flightId &&
            x.SeatNumber.Equals(seatNumber, StringComparison.OrdinalIgnoreCase) &&
            x.UserId == userId);

        if (hold is null || hold.Status != "Locked" || hold.ReservedUntilUtc <= nowUtc)
            return Task.FromResult<SeatHold?>(null);

        hold.Status = "Reserved";
        hold.LastUpdatedUtc = nowUtc;
        return Task.FromResult<SeatHold?>(Clone(hold));
    }

    public Task<bool> ReleaseSeatAsync(string flightId, string seatNumber, string? userId, bool allowAnyUser, DateTime nowUtc)
    {
        var hold = Holds.FirstOrDefault(x =>
            x.FlightId == flightId &&
            x.SeatNumber.Equals(seatNumber, StringComparison.OrdinalIgnoreCase));

        if (hold is null)
            return Task.FromResult(false);

        if (!allowAnyUser && !string.Equals(hold.UserId, userId, StringComparison.Ordinal))
            return Task.FromResult(false);

        if (hold.Status == "Locked" && hold.ReservedUntilUtc <= nowUtc)
            return Task.FromResult(false);

        if (hold.Status != "Locked" && hold.Status != "Reserved")
            return Task.FromResult(false);

        hold.Status = "Released";
        hold.ReservedUntilUtc = nowUtc;
        hold.LastUpdatedUtc = nowUtc;
        return Task.FromResult(true);
    }

    public Task UpdateAsync(SeatHold hold)
    {
        var existing = Holds.FirstOrDefault(x => x.Id == hold.Id);

        if (existing != null)
        {
            Holds.Remove(existing);
            Holds.Add(Clone(hold));
        }

        return Task.CompletedTask;
    }

    private static SeatHold Clone(SeatHold hold)
    {
        return new SeatHold
        {
            Id = hold.Id,
            FlightId = hold.FlightId,
            SeatNumber = hold.SeatNumber,
            UserId = hold.UserId,
            ReservedUntilUtc = hold.ReservedUntilUtc,
            Status = hold.Status,
            LastUpdatedUtc = hold.LastUpdatedUtc
        };
    }
}