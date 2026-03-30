using AvailabilityService.Application.Repositories;
using AvailabilityService.Domain.Entities;

namespace AvailabilityService.Infrastructure.Repositories;

public class InMemorySeatHoldRepository : ISeatHoldRepository
{
    private readonly List<SeatHold> _holds = new();

    public Task AddAsync(SeatHold hold)
    {
        _holds.Add(hold);
        return Task.CompletedTask;
    }

    public Task<SeatHold?> GetByIdAsync(string id)
    {
        var hold = _holds.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(hold);
    }

    public Task UpdateAsync(SeatHold hold)
    {
        var existing = _holds.FirstOrDefault(x => x.Id == hold.Id);

        if (existing != null)
        {
            _holds.Remove(existing);
            _holds.Add(hold);
        }

        return Task.CompletedTask;
    }
}