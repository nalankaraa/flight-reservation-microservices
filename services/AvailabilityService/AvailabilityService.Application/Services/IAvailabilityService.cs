using AvailabilityService.Application.Dtos;

namespace AvailabilityService.Application.Services;

public interface IAvailabilityService
{
    Task<SeatHoldResponseDto> CreateHoldAsync(CreateSeatHoldDto request);
    Task<SeatHoldResponseDto?> GetHoldByIdAsync(string id);
    Task<bool> ConfirmHoldAsync(string id);
    Task<bool> CancelHoldAsync(string id);
}