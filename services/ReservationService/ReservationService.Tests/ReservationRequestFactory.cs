using ReservationService.Application.Dtos;

namespace ReservationService.Tests;

public static class ReservationRequestFactory
{
    public static CreateReservationDto Create(
        string flightId = "flight-1",
        string passengerName = "Ali",
        string seatNumber = "10B")
    {
        return new CreateReservationDto
        {
            FlightId = flightId,
            PassengerName = passengerName,
            SeatNumber = seatNumber
        };
    }
}
