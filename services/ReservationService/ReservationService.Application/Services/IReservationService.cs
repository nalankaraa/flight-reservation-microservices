using ReservationService.Application.Dtos;

namespace ReservationService.Application.Services;

public interface IReservationService
{
    Task<ReservationResponseDto> CreateAsync(CreateReservationDto request);
    Task<List<ReservationResponseDto>> GetAllAsync();
}