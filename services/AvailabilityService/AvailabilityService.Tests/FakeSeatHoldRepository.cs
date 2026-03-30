using AvailabilityService.Application.Repositories;
using AvailabilityService.Domain.Entities;

namespace AvailabilityService.Tests;

public class FakeSeatHoldRepository : ISeatHoldRepository
{
    public List<SeatHold> Holds { get; } = new();

    public Task AddAsync(SeatHold hold)
    {
        Holds.Add(hold);
        return Task.CompletedTask;
    }

    public Task<SeatHold?> GetByIdAsync(string id)
    {
        return Task.FromResult(Holds.FirstOrDefault(x => x.Id == id));
    }

    public Task UpdateAsync(SeatHold hold)
    {
        var existing = Holds.FirstOrDefault(x => x.Id == hold.Id);

        if (existing != null)
        {
            Holds.Remove(existing);
            Holds.Add(hold);
        }

        return Task.CompletedTask;
    }
}