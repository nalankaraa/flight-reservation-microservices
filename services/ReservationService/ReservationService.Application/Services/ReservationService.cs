using ReservationService.Application.Clients;
using ReservationService.Application.Dtos;
using ReservationService.Application.Repositories;
using ReservationService.Domain.Entities;

namespace ReservationService.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repository;
    private readonly ISeatAvailabilityClient _seatAvailabilityClient;
    private const int SeatLockMinutes = 10;

    public ReservationService(IReservationRepository repository, ISeatAvailabilityClient seatAvailabilityClient)
    {
        _repository = repository;
        _seatAvailabilityClient = seatAvailabilityClient;
    }

    public async Task<ReservationResponseDto> CreateAsync(CreateReservationDto request, string userId, string authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(request.FlightId) ||
            string.IsNullOrWhiteSpace(request.PassengerName) ||
            string.IsNullOrWhiteSpace(request.SeatNumber) ||
            string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return new ReservationResponseDto
            {
                Success = false,
                Message = "FlightId, PassengerName, SeatNumber and authorization are required."
            };
        }

        var normalizedSeatNumber = request.SeatNumber.Trim().ToUpperInvariant();

        if (await _repository.ExistsByFlightAndSeatAsync(request.FlightId, normalizedSeatNumber))
        {
            return BuildSeatConflict(request.FlightId, normalizedSeatNumber);
        }

        var seatLock = await _seatAvailabilityClient.LockSeatAsync(
            request.FlightId,
            normalizedSeatNumber,
            SeatLockMinutes,
            authorizationHeader);

        if (seatLock.IsConflict)
        {
            return BuildSeatConflict(request.FlightId, normalizedSeatNumber);
        }

        if (!seatLock.Success)
        {
            return new ReservationResponseDto
            {
                Success = false,
                ErrorCode = "AvailabilityUnavailable",
                FlightId = request.FlightId,
                SeatNumber = normalizedSeatNumber,
                Message = "Seat availability service is unavailable."
            };
        }

        var reservation = new Reservation
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            FlightId = request.FlightId,
            PassengerName = request.PassengerName,
            SeatNumber = normalizedSeatNumber
        };

        await _repository.AddAsync(reservation);

        var seatConfirmation = await _seatAvailabilityClient.ConfirmSeatAsync(
            request.FlightId,
            normalizedSeatNumber,
            authorizationHeader);

        if (!seatConfirmation.Success)
        {
            await _repository.DeleteAsync(reservation.Id);
            await _seatAvailabilityClient.ReleaseSeatAsync(request.FlightId, normalizedSeatNumber, authorizationHeader);

            return new ReservationResponseDto
            {
                Success = false,
                ErrorCode = seatConfirmation.IsConflict ? "SeatConflict" : "AvailabilityUnavailable",
                FlightId = request.FlightId,
                SeatNumber = normalizedSeatNumber,
                Message = seatConfirmation.IsConflict
                    ? "The selected seat could not be confirmed."
                    : "Seat availability service is unavailable."
            };
        }

        return new ReservationResponseDto
        {
            Success = true,
            Id = reservation.Id,
            UserId = reservation.UserId,
            FlightId = reservation.FlightId,
            PassengerName = reservation.PassengerName,
            SeatNumber = reservation.SeatNumber,
            Message = "Reservation created successfully."
        };
    }

    public async Task<List<ReservationResponseDto>> GetAllAsync()
    {
        var reservations = await _repository.GetAllAsync();

        return reservations.Select(r => new ReservationResponseDto
        {
            Success = true,
            Id = r.Id,
            UserId = r.UserId,
            FlightId = r.FlightId,
            PassengerName = r.PassengerName,
            SeatNumber = r.SeatNumber
        }).ToList();
    }

    public async Task<List<ReservationResponseDto>> GetMineAsync(string userId)
    {
        var reservations = await _repository.GetByUserIdAsync(userId);

        return reservations.Select(r => new ReservationResponseDto
        {
            Success = true,
            Id = r.Id,
            UserId = r.UserId,
            FlightId = r.FlightId,
            PassengerName = r.PassengerName,
            SeatNumber = r.SeatNumber
        }).ToList();
    }

    private static ReservationResponseDto BuildSeatConflict(string flightId, string seatNumber)
    {
        return new ReservationResponseDto
        {
            Success = false,
            ErrorCode = "SeatConflict",
            FlightId = flightId,
            SeatNumber = seatNumber,
            Message = "The selected seat is already reserved."
        };
    }
}