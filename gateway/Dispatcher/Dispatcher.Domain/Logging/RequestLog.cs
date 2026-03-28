namespace Dispatcher.Domain.Logging;
public class RequestLog
{
    public string Id { get; set; } = default!;
    public DateTime TimestampUtc { get; set; }
    public string Path { get; set; } = default!;
    public string Method { get; set; } = default!;
    public int StatusCode { get; set; }
    public double DurationMs { get; set; }
    public string? TargetService { get; set; }
    public string? ErrorMessage { get; set; }
}