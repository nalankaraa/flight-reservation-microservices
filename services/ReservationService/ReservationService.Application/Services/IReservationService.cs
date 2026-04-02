using ReservationService.Application.Dtos;

namespace ReservationService.Application.Services;

public interface IReservationService
{
    Task<ReservationResponseDto> CreateAsync(CreateReservationDto request, string userId);
    Task<List<ReservationResponseDto>> GetAllAsync();
    Task<List<ReservationResponseDto>> GetMineAsync(string userId);
}
