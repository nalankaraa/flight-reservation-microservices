using AvailabilityService.Domain.Entities;

namespace AvailabilityService.Application.Repositories;

public interface ISeatHoldRepository
{
    Task AddAsync(SeatHold hold);
    Task<SeatHold?> GetByIdAsync(string id);
    Task UpdateAsync(SeatHold hold);
}