using ReservationService.Application.Dtos;
using ReservationService.Application.Repositories;

namespace ReservationService.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repository;

    public ReservationService(IReservationRepository repository)
    {
        _repository = repository;
    }

    public Task<ReservationResponseDto> CreateAsync(CreateReservationDto request)
    {
        
        return Task.FromResult(new ReservationResponseDto());
    }

    public Task<List<ReservationResponseDto>> GetAllAsync()
    {
        
        return Task.FromResult(new List<ReservationResponseDto>());
    }
}