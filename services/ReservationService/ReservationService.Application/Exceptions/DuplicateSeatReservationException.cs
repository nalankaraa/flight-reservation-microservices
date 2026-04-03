namespace ReservationService.Application.Exceptions;

public class DuplicateSeatReservationException : Exception
{
    public DuplicateSeatReservationException(string message) : base(message)
    {
    }
}
