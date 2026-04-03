using ReservationService.Application.Clients;

namespace ReservationService.Tests;

public class FakeSeatAvailabilityClient : ISeatAvailabilityClient
{
    public bool LockShouldConflict { get; set; }
    public bool LockShouldBeUnavailable { get; set; }
    public bool ConfirmShouldConflict { get; set; }
    public bool ConfirmShouldBeUnavailable { get; set; }
    public int LockCallCount { get; private set; }
    public int ConfirmCallCount { get; private set; }
    public int ReleaseCallCount { get; private set; }

    public Task<SeatLockResult> LockSeatAsync(string flightId, string seatNumber, int holdMinutes, string authorizationHeader)
    {
        LockCallCount++;

        if (LockShouldConflict)
        {
            return Task.FromResult(new SeatLockResult
            {
                IsConflict = true
            });
        }

        if (LockShouldBeUnavailable)
        {
            return Task.FromResult(new SeatLockResult
            {
                IsServiceUnavailable = true
            });
        }

        return Task.FromResult(new SeatLockResult
        {
            Success = true
        });
    }

    public Task<SeatLockResult> ConfirmSeatAsync(string flightId, string seatNumber, string authorizationHeader)
    {
        ConfirmCallCount++;

        if (ConfirmShouldConflict)
        {
            return Task.FromResult(new SeatLockResult
            {
                IsConflict = true
            });
        }

        if (ConfirmShouldBeUnavailable)
        {
            return Task.FromResult(new SeatLockResult
            {
                IsServiceUnavailable = true
            });
        }

        return Task.FromResult(new SeatLockResult
        {
            Success = true
        });
    }

    public Task ReleaseSeatAsync(string flightId, string seatNumber, string authorizationHeader)
    {
        ReleaseCallCount++;
        return Task.CompletedTask;
    }
}