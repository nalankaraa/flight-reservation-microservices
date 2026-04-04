using System.Collections.Concurrent;
using System.Text;

namespace Dispatcher.Api.Observability;

public class DispatcherMetricsStore
{
    private static readonly double[] DurationBuckets = [5, 10, 25, 50, 100, 250, 500, 1000, 2500, 5000];
    private readonly ConcurrentDictionary<RequestMetricKey, long> _requestCounts = new();
    private readonly ConcurrentDictionary<DurationMetricKey, DurationHistogram> _durationHistograms = new();

    public void RecordRequest(string path, string method, int statusCode, double durationMs, string? targetService)
    {
        var normalizedPath = Normalize(path);
        var normalizedMethod = Normalize(method);
        var normalizedTargetService = Normalize(targetService ?? "unknown");
        var statusCodeText = statusCode.ToString();
        var statusClass = $"{statusCode / 100}xx";

        _requestCounts.AddOrUpdate(
            new RequestMetricKey(normalizedPath, normalizedMethod, normalizedTargetService, statusCodeText, statusClass),
            1,
            (_, existing) => existing + 1);

        var histogram = _durationHistograms.GetOrAdd(
            new DurationMetricKey(normalizedPath, normalizedMethod, normalizedTargetService),
            _ => new DurationHistogram(DurationBuckets));

        histogram.Observe(durationMs);
    }

    public string RenderPrometheus()
    {
        var sb = new StringBuilder();

        sb.AppendLine("# HELP dispatcher_requests_total Total number of requests handled by the dispatcher.");
        sb.AppendLine("# TYPE dispatcher_requests_total counter");

        foreach (var item in _requestCounts.OrderBy(x => x.Key.Path).ThenBy(x => x.Key.Method))
        {
            sb.Append("dispatcher_requests_total{");
            AppendLabel(sb, "path", item.Key.Path);
            sb.Append(',');
            AppendLabel(sb, "method", item.Key.Method);
            sb.Append(',');
            AppendLabel(sb, "target_service", item.Key.TargetService);
            sb.Append(',');
            AppendLabel(sb, "status_code", item.Key.StatusCode);
            sb.Append(',');
            AppendLabel(sb, "status_class", item.Key.StatusClass);
            sb.AppendLine($"}} {item.Value}");
        }

        sb.AppendLine("# HELP dispatcher_request_duration_ms Request duration in milliseconds.");
        sb.AppendLine("# TYPE dispatcher_request_duration_ms histogram");

        foreach (var item in _durationHistograms.OrderBy(x => x.Key.Path).ThenBy(x => x.Key.Method))
        {
            var snapshot = item.Value.Snapshot();
            long cumulative = 0;

            for (var i = 0; i < DurationBuckets.Length; i++)
            {
                cumulative += snapshot.BucketCounts[i];
                sb.Append("dispatcher_request_duration_ms_bucket{");
                AppendBaseDurationLabels(sb, item.Key);
                sb.Append(',');
                AppendLabel(sb, "le", DurationBuckets[i].ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine($"}} {cumulative}");
            }

            sb.Append("dispatcher_request_duration_ms_bucket{");
            AppendBaseDurationLabels(sb, item.Key);
            sb.Append(',');
            AppendLabel(sb, "le", "+Inf");
            sb.AppendLine($"}} {snapshot.Count}");

            sb.Append("dispatcher_request_duration_ms_sum{");
            AppendBaseDurationLabels(sb, item.Key);
            sb.AppendLine($"}} {snapshot.Sum.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}");

            sb.Append("dispatcher_request_duration_ms_count{");
            AppendBaseDurationLabels(sb, item.Key);
            sb.AppendLine($"}} {snapshot.Count}");
        }

        return sb.ToString();
    }

    private static void AppendBaseDurationLabels(StringBuilder sb, DurationMetricKey key)
    {
        AppendLabel(sb, "path", key.Path);
        sb.Append(',');
        AppendLabel(sb, "method", key.Method);
        sb.Append(',');
        AppendLabel(sb, "target_service", key.TargetService);
    }

    private static void AppendLabel(StringBuilder sb, string name, string value)
    {
        sb.Append(name);
        sb.Append("=\"");
        sb.Append(value.Replace("\\", "\\\\").Replace("\"", "\\\""));
        sb.Append('"');
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
    }

    private sealed class DurationHistogram
    {
        private readonly object _lock = new();
        private readonly double[] _buckets;
        private readonly long[] _bucketCounts;
        private double _sum;
        private long _count;

        public DurationHistogram(double[] buckets)
        {
            _buckets = buckets;
            _bucketCounts = new long[buckets.Length];
        }

        public void Observe(double value)
        {
            lock (_lock)
            {
                _count++;
                _sum += value;

                for (var i = 0; i < _buckets.Length; i++)
                {
                    if (value <= _buckets[i])
                    {
                        _bucketCounts[i]++;
                    }
                }
            }
        }

        public HistogramSnapshot Snapshot()
        {
            lock (_lock)
            {
                return new HistogramSnapshot((long[])_bucketCounts.Clone(), _sum, _count);
            }
        }
    }

    private sealed record HistogramSnapshot(long[] BucketCounts, double Sum, long Count);
    private sealed record RequestMetricKey(string Path, string Method, string TargetService, string StatusCode, string StatusClass);
    private sealed record DurationMetricKey(string Path, string Method, string TargetService);
}
