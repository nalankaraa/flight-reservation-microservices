namespace Dispatcher.Domain.Logging;

public class RequestLog
{
    public string Path { get; set; } = default!;
    public string Method { get; set; } = default!;
    public int StatusCode { get; set; }
    public double DurationMs { get; set; }
    public DateTime TimestampUtc { get; set; }
}