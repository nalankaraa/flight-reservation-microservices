using ReservationService.Application.Dtos;
using ReservationService.Application.Repositories;
using ReservationService.Domain.Entities;

namespace ReservationService.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repository;

    public ReservationService(IReservationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ReservationResponseDto> CreateAsync(CreateReservationDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FlightId) ||
            string.IsNullOrWhiteSpace(request.PassengerName) ||
            string.IsNullOrWhiteSpace(request.SeatNumber))
        {
            return new ReservationResponseDto
            {
                Success = false,
                Message = "FlightId, PassengerName and SeatNumber are required."
            };
        }

        var reservation = new Reservation
        {
            Id = Guid.NewGuid().ToString(),
            FlightId = request.FlightId,
            PassengerName = request.PassengerName,
            SeatNumber = request.SeatNumber
        };

        await _repository.AddAsync(reservation);

        return new ReservationResponseDto
        {
            Success = true,
            Id = reservation.Id,
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
            FlightId = r.FlightId,
            PassengerName = r.PassengerName,
            SeatNumber = r.SeatNumber
        }).ToList();
    }
}