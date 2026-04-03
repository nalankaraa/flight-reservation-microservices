using ReservationService.Application.Dtos;

namespace ReservationService.Application.Services;

public interface IReservationService
{
    Task<ReservationResponseDto> CreateAsync(CreateReservationDto request, string userId, string authorizationHeader);
    Task<List<ReservationResponseDto>> GetAllAsync();
    Task<ReservationResponseDto?> GetByIdAsync(string id);
    Task<List<ReservationResponseDto>> GetMineAsync(string userId);
}
