using Dispatcher.Application.Logging;
using Dispatcher.Domain.Logging;

namespace Dispatcher.Infrastructure.Logging;

public class InMemoryRequestLogRepository : IRequestLogRepository
{
    private readonly List<RequestLog> _logs = new();

    public Task AddAsync(RequestLog log)
    {
        _logs.Add(log);
        return Task.CompletedTask;
    }

    public Task<List<RequestLog>> GetRecentAsync(int count = 100)
    {
        var result = _logs
            .OrderByDescending(x => x.TimestampUtc)
            .Take(count)
            .ToList();

        return Task.FromResult(result);
    }
}