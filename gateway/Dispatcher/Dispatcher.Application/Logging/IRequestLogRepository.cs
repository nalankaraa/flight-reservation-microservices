using Dispatcher.Domain.Logging;
namespace Dispatcher.Application.Logging;
public interface IRequestLogRepository
{
    Task AddAsync(RequestLog log);
    Task<List<RequestLog>> GetRecentAsync(int count = 100);
}