namespace ReservationService.Application.Clients;

public interface ISeatAvailabilityClient
{
    Task<SeatLockResult> LockSeatAsync(string flightId, string seatNumber, int holdMinutes, string authorizationHeader);
    Task<SeatLockResult> ConfirmSeatAsync(string flightId, string seatNumber, string authorizationHeader);
    Task ReleaseSeatAsync(string flightId, string seatNumber, string authorizationHeader);
}