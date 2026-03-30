using AvailabilityService.Application.Dtos;
using AvailabilityService.Application.Repositories;
using AvailabilityService.Domain.Entities;

namespace AvailabilityService.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly ISeatHoldRepository _repository;

    public AvailabilityService(ISeatHoldRepository repository)
    {
        _repository = repository;
    }

    public async Task<SeatHoldResponseDto> CreateHoldAsync(CreateSeatHoldDto request)
    {
        var hold = new SeatHold
        {
            FlightId = request.FlightId,
            UserId = request.UserId,
            SeatCount = request.SeatCount,
            ReservedUntilUtc = DateTime.UtcNow.AddMinutes(request.HoldMinutes),
            Status = "Pending"
        };

        await _repository.AddAsync(hold);

        return MapToDto(hold);
    }

    public async Task<SeatHoldResponseDto?> GetHoldByIdAsync(string id)
    {
        var hold = await _repository.GetByIdAsync(id);

        if (hold is null)
            return null;

        if (IsExpired(hold))
        {
            hold.Status = "Expired";
            await _repository.UpdateAsync(hold);
        }

        return MapToDto(hold);
    }

    public async Task<bool> ConfirmHoldAsync(string id)
    {
        var hold = await _repository.GetByIdAsync(id);

        if (hold is null)
            return false;

        if (IsExpired(hold))
        {
            hold.Status = "Expired";
            await _repository.UpdateAsync(hold);
            return false;
        }

        if (hold.Status != "Pending")
            return false;

        hold.Status = "Confirmed";
        await _repository.UpdateAsync(hold);

        return true;
    }

    public async Task<bool> CancelHoldAsync(string id)
    {
        var hold = await _repository.GetByIdAsync(id);

        if (hold is null)
            return false;

        if (IsExpired(hold))
        {
            hold.Status = "Expired";
            await _repository.UpdateAsync(hold);
            return false;
        }

        if (hold.Status != "Pending")
            return false;

        hold.Status = "Cancelled";
        await _repository.UpdateAsync(hold);

        return true;
    }

    private static bool IsExpired(SeatHold hold)
    {
        return hold.ReservedUntilUtc <= DateTime.UtcNow;
    }

    private static SeatHoldResponseDto MapToDto(SeatHold hold)
    {
        return new SeatHoldResponseDto
        {
            Id = hold.Id,
            FlightId = hold.FlightId,
            UserId = hold.UserId,
            SeatCount = hold.SeatCount,
            ReservedUntilUtc = hold.ReservedUntilUtc,
            Status = hold.Status
        };
    }
}