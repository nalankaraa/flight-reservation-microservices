using System.Threading.Channels;
using Dispatcher.Application.Logging;
using Dispatcher.Domain.Logging;
using Microsoft.Extensions.Hosting;

namespace Dispatcher.Infrastructure.Logging;

public class BufferedRequestLogRepository : IRequestLogRepository, IHostedService
{
    private readonly MongoRequestLogRepository _innerRepository;
    private readonly Channel<RequestLog> _channel;
    private readonly CancellationTokenSource _shutdown = new();
    private Task? _worker;

    public BufferedRequestLogRepository(MongoRequestLogRepository innerRepository)
    {
        _innerRepository = innerRepository;
        _channel = Channel.CreateBounded<RequestLog>(new BoundedChannelOptions(20_000)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    public Task AddAsync(RequestLog log)
    {
        _channel.Writer.TryWrite(log);
        return Task.CompletedTask;
    }

    public Task<List<RequestLog>> GetRecentAsync(int count = 100)
    {
        return _innerRepository.GetRecentAsync(count);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _worker = Task.Run(ProcessQueueAsync, cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();
        _shutdown.Cancel();

        if (_worker is not null)
        {
            await Task.WhenAny(_worker, Task.Delay(TimeSpan.FromSeconds(5), cancellationToken));
        }
    }

    private async Task ProcessQueueAsync()
    {
        var buffer = new List<RequestLog>(128);

        while (await _channel.Reader.WaitToReadAsync(_shutdown.Token))
        {
            buffer.Clear();

            while (buffer.Count < 128 && _channel.Reader.TryRead(out var log))
            {
                buffer.Add(log);
            }

            if (buffer.Count == 0)
            {
                continue;
            }

            await _innerRepository.AddManyAsync(buffer, _shutdown.Token);
        }
    }
}
