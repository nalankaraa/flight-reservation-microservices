using Dispatcher.Application.Logging;
using Dispatcher.Domain.Logging;

namespace Dispatcher.Tests;

public class FakeRequestLogRepository : IRequestLogRepository
{
    public List<RequestLog> Logs { get; } = new();

    public Task AddAsync(RequestLog log)
    {
        Logs.Add(log);
        return Task.CompletedTask;
    }

    public Task<List<RequestLog>> GetRecentAsync(int count = 100)
    {
        return Task.FromResult(Logs.Take(count).ToList());
    }
}