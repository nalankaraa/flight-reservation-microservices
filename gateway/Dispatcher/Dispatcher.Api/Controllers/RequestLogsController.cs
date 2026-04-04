using Dispatcher.Application.Logging;
using Dispatcher.Domain.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api/dispatcher/logs")]
[Authorize(Roles = "Admin")]
public class RequestLogsController : ControllerBase
{
    private static readonly string RealisticLoadTestResultsPath = Path.Combine("monitoring", "load-tests", "results-realistic.json");

    private readonly IRequestLogRepository _requestLogRepository;
    private readonly IHostEnvironment _hostEnvironment;

    public RequestLogsController(IRequestLogRepository requestLogRepository, IHostEnvironment hostEnvironment)
    {
        _requestLogRepository = requestLogRepository;
        _hostEnvironment = hostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 100)
    {
        var normalizedCount = Math.Clamp(count, 1, 500);
        var logs = await _requestLogRepository.GetRecentAsync(normalizedCount);
        return Ok(logs);
    }

    [HttpGet("/api/dispatcher/load-tests")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLoadTestResults()
    {
        var results = await ReadLoadTestResultsAsync();
        return Ok(results);
    }

    [HttpGet("/dashboard/login")]
    [AllowAnonymous]
    public IActionResult DashboardLogin([FromQuery] string? target = null)
    {
        var normalizedTarget = string.IsNullOrWhiteSpace(target)
            ? "/dashboard"
            : target;

        return Content(BuildDashboardLoginHtml(normalizedTarget), "text/html; charset=utf-8");
    }

    [HttpGet("/dashboard/json/logs")]
    [AllowAnonymous]
    public IActionResult DashboardLogsJsonView([FromQuery] int count = 200)
    {
        var normalizedCount = Math.Clamp(count, 1, 500);
        return Content(BuildDashboardLogsJsonHtml(normalizedCount), "text/html; charset=utf-8");
    }

    [HttpGet("/dashboard")]
    [HttpGet("/dispatcher/dashboard")]
    [AllowAnonymous]
    public async Task<IActionResult> Dashboard([FromQuery] int count = 100)
    {
        var normalizedCount = Math.Clamp(count, 1, 500);
        var logs = await _requestLogRepository.GetRecentAsync(normalizedCount);
        var loadTestResults = await ReadLoadTestResultsAsync();
        var html = BuildDashboardHtml(
            logs,
            normalizedCount,
            "http://localhost:3000",
            loadTestResults);

        return Content(html, "text/html; charset=utf-8");
    }

    private async Task<List<LoadTestResult>> ReadLoadTestResultsAsync()
    {
        var path = Path.Combine(_hostEnvironment.ContentRootPath, RealisticLoadTestResultsPath);

        if (!System.IO.File.Exists(path))
            return [];

        try
        {
            await using var stream = System.IO.File.OpenRead(path);
            var results = await JsonSerializer.DeserializeAsync<List<LoadTestResult>>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return results ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string BuildDashboardHtml(
        List<RequestLog> logs,
        int count,
        string grafanaUrl,
        List<LoadTestResult> loadTestResults)
    {
        var totalRequests = logs.Count;
        var successCount = logs.Count(x => x.StatusCode is >= 200 and < 400);
        var errorCount = logs.Count(x => x.StatusCode >= 400);
        var averageDuration = totalRequests == 0 ? 0 : logs.Average(x => x.DurationMs);
        var errorRate = totalRequests == 0 ? 0 : (double)errorCount / totalRequests * 100;
        var services = logs
            .Select(x => x.TargetService)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        var logRows = BuildLogRows(logs);
        var serviceRows = BuildServiceRows(logs);
        var loadTestRows = BuildLoadTestRows(loadTestResults);
        var loadTestCharts = BuildLoadTestCharts(loadTestResults);

        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>Dispatcher Monitoring Console</title>
                <style>
                    :root {
                        color-scheme: light;
                        --bg: #f5efe2;
                        --panel: #fffdf8;
                        --ink: #1e1b18;
                        --muted: #6f655c;
                        --line: #d8cfc3;
                        --accent: #0f766e;
                        --danger: #b42318;
                        --warn: #b54708;
                        --shadow: 0 18px 40px rgba(56, 44, 30, 0.12);
                    }

                    * { box-sizing: border-box; }

                    html { scroll-behavior: smooth; }

                    body {
                        margin: 0;
                        font-family: "Segoe UI", Tahoma, sans-serif;
                        background:
                            radial-gradient(circle at top left, rgba(15, 118, 110, 0.12), transparent 28%),
                            linear-gradient(180deg, #f8f2e8 0%, var(--bg) 100%);
                        color: var(--ink);
                    }

                    .wrap {
                        width: min(1340px, calc(100% - 32px));
                        margin: 24px auto 40px;
                    }

                    .hero, .panel {
                        background: var(--panel);
                        border: 1px solid var(--line);
                        border-radius: 24px;
                        box-shadow: var(--shadow);
                    }

                    .hero {
                        padding: 28px;
                    }

                    h1 {
                        margin: 0 0 8px;
                        font-size: clamp(2rem, 3vw, 3.2rem);
                        line-height: 1.05;
                    }

                    .sub {
                        margin: 0;
                        color: var(--muted);
                        font-size: 1rem;
                        max-width: 980px;
                    }

                    .tabs {
                        display: flex;
                        gap: 12px;
                        flex-wrap: wrap;
                        margin-top: 22px;
                    }

                    .tabs a {
                        text-decoration: none;
                        color: var(--ink);
                        background: rgba(15, 118, 110, 0.07);
                        border: 1px solid rgba(15, 118, 110, 0.15);
                        padding: 10px 16px;
                        border-radius: 999px;
                        font-weight: 700;
                    }

                    .stats {
                        display: grid;
                        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
                        gap: 16px;
                        margin-top: 22px;
                    }

                    .card {
                        background: #fcfaf5;
                        border: 1px solid var(--line);
                        border-radius: 18px;
                        padding: 18px;
                    }

                    .label {
                        display: block;
                        color: var(--muted);
                        font-size: 0.85rem;
                        margin-bottom: 6px;
                    }

                    .value {
                        font-size: 1.9rem;
                        font-weight: 700;
                    }

                    .section {
                        margin-top: 22px;
                        scroll-margin-top: 20px;
                    }

                    .panel-head {
                        display: flex;
                        justify-content: space-between;
                        gap: 16px;
                        align-items: start;
                        padding: 22px 24px 0;
                    }

                    .section-title {
                        margin: 0 0 8px;
                        font-size: 1.2rem;
                    }

                    .section-copy {
                        margin: 0;
                        color: var(--muted);
                    }

                    .hint {
                        color: var(--muted);
                        font-size: 0.92rem;
                    }

                    .stack {
                        display: flex;
                        flex-direction: column;
                        gap: 22px;
                    }

                    .quick-links {
                        padding: 22px 24px;
                    }

                    .quick-links p {
                        margin: 0 0 16px;
                        color: var(--muted);
                    }

                    .links {
                        display: flex;
                        flex-wrap: wrap;
                        gap: 12px;
                    }

                    .links a {
                        text-decoration: none;
                        color: var(--accent);
                        background: rgba(15, 118, 110, 0.08);
                        border: 1px solid rgba(15, 118, 110, 0.16);
                        padding: 10px 14px;
                        border-radius: 999px;
                        font-weight: 600;
                    }

                    .overview-layout {
                        padding: 22px 24px 24px;
                    }

                    .table-wrap {
                        overflow-x: auto;
                        padding: 0 24px 24px;
                    }

                    .log-table-wrap {
                        max-height: 520px;
                        overflow: auto;
                        border-top: 1px solid #eee4d7;
                    }

                    .load-grid {
                        display: grid;
                        grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
                        gap: 16px;
                        padding: 22px 24px 0;
                    }

                    .load-card {
                        background: #fcfaf5;
                        border: 1px solid var(--line);
                        border-radius: 18px;
                        padding: 18px;
                    }

                    .load-card h3 {
                        margin: 0 0 12px;
                        font-size: 1.05rem;
                    }

                    .metric-row {
                        margin-top: 12px;
                    }

                    .metric-line {
                        display: flex;
                        justify-content: space-between;
                        gap: 12px;
                        font-size: 0.92rem;
                        margin-bottom: 6px;
                    }

                    .metric-bar {
                        height: 10px;
                        border-radius: 999px;
                        background: #efe6d8;
                        overflow: hidden;
                    }

                    .metric-fill {
                        height: 100%;
                        border-radius: inherit;
                        background: linear-gradient(90deg, #0f766e, #14b8a6);
                    }

                    .metric-fill.warn {
                        background: linear-gradient(90deg, #b54708, #f97316);
                    }

                    .metric-fill.danger {
                        background: linear-gradient(90deg, #b42318, #ef4444);
                    }

                    .load-note {
                        margin-top: 12px;
                        color: var(--muted);
                        font-size: 0.88rem;
                        line-height: 1.45;
                    }

                    table {
                        width: 100%;
                        border-collapse: collapse;
                        min-width: 900px;
                    }

                    th, td {
                        text-align: left;
                        padding: 14px 16px;
                        border-bottom: 1px solid #eee4d7;
                        vertical-align: top;
                        font-size: 0.94rem;
                    }

                    th {
                        background: #f7f1e6;
                        color: var(--muted);
                        font-size: 0.78rem;
                        text-transform: uppercase;
                        letter-spacing: 0.08em;
                        position: sticky;
                        top: 0;
                    }

                    .method {
                        display: inline-block;
                        padding: 4px 10px;
                        border-radius: 999px;
                        background: rgba(15, 118, 110, 0.1);
                        color: var(--accent);
                        font-weight: 700;
                    }

                    .status {
                        display: inline-block;
                        min-width: 56px;
                        text-align: center;
                        padding: 4px 10px;
                        border-radius: 999px;
                        font-weight: 700;
                    }

                    .status-2xx { background: rgba(15, 118, 110, 0.12); color: var(--accent); }
                    .status-4xx { background: rgba(181, 71, 8, 0.14); color: var(--warn); }
                    .status-5xx { background: rgba(180, 35, 24, 0.14); color: var(--danger); }

                    .empty {
                        text-align: center;
                        color: var(--muted);
                        padding: 28px;
                    }

                    .traffic-panel {
                        padding: 22px 24px 24px;
                    }

                    .analytics-grid {
                        display: grid;
                        grid-template-columns: repeat(12, minmax(0, 1fr));
                        gap: 16px;
                    }

                    .analytics-card {
                        background: #17181d;
                        border: 1px solid var(--line);
                        border-radius: 18px;
                        overflow: hidden;
                    }

                    .analytics-card.stat {
                        grid-column: span 4;
                        min-height: 220px;
                    }

                    .analytics-card.chart {
                        grid-column: span 6;
                        min-height: 420px;
                    }

                    iframe {
                        width: 100%;
                        border: 0;
                        display: block;
                    }

                    .analytics-card.stat iframe {
                        min-height: 220px;
                    }

                    .analytics-card.chart iframe {
                        min-height: 420px;
                    }

                    @media (max-width: 1100px) {
                        .analytics-card.stat,
                        .analytics-card.chart {
                            grid-column: span 12;
                        }
                    }

                    @media (max-width: 720px) {
                        .wrap {
                            width: min(100% - 20px, 1340px);
                        }

                        .hero, .panel {
                            border-radius: 18px;
                        }

                        .panel-head {
                            flex-direction: column;
                        }
                    }
                </style>
            </head>
            <body>
                <div class="wrap">
                    <section class="hero">
                        <h1>Dispatcher Monitoring Console</h1>
                        <p class="sub">Single entry-point interface for overview metrics, detailed request logs, Grafana traffic analytics and load test results required by the project specification.</p>

                        <div class="stats">
                            <article class="card">
                                <span class="label">Total Requests</span>
                                <span class="value">{{totalRequests}}</span>
                            </article>
                            <article class="card">
                                <span class="label">Successful Requests</span>
                                <span class="value">{{successCount}}</span>
                            </article>
                            <article class="card">
                                <span class="label">Failed Requests</span>
                                <span class="value">{{errorCount}}</span>
                            </article>
                            <article class="card">
                                <span class="label">Average Response Time</span>
                                <span class="value">{{averageDuration.ToString("F1")}} ms</span>
                            </article>
                            <article class="card">
                                <span class="label">Service Count</span>
                                <span class="value">{{services}}</span>
                            </article>
                            <article class="card">
                                <span class="label">Error Rate</span>
                                <span class="value">{{errorRate.ToString("F1")}}%</span>
                            </article>
                        </div>

                        <nav class="tabs">
                            <a href="#overview">Overview</a>
                            <a href="#logs">Logs</a>
                            <a href="#traffic">Traffic Analytics</a>
                            <a href="#load-tests">Load Test Results</a>
                        </nav>
                    </section>

                    <section id="overview" class="panel section">
                        <div class="panel-head">
                            <div>
                                <h2 class="section-title">Overview</h2>
                                <p class="section-copy">Top-level request health, service distribution and quick access into the monitoring stack based on recent dispatcher logs.</p>
                            </div>
                            <span class="hint">Latest {{count}} requests sampled from MongoDB logs.</span>
                        </div>

                        <div class="overview-layout">
                            <div class="stack">
                                <section class="quick-links panel">
                                    <h2 class="section-title">Monitoring Links</h2>
                                    <p>The cards above are calculated from recent MongoDB request logs. Load test benchmarks are shown separately in the dedicated section below and read only from the latest realistic workflow result file.</p>
                                    <div class="links">
                                        <a href="/">Refresh Home</a>
                                        <a href="/dashboard?count=200">Show 200 Logs</a>
                                        <a href="http://localhost:3000/d/dispatcher-observability/dispatcher-observability" target="_blank" rel="noreferrer">Open Full Grafana</a>
                                        <a href="/dashboard/login?target=%2Fdashboard%2Fjson%2Flogs%3Fcount%3D200">JSON Logs API</a>
                                        <a href="/api/dispatcher/load-tests" target="_blank" rel="noreferrer">JSON Load Tests API</a>
                                    </div>
                                </section>

                                <section class="panel">
                                    <div class="panel-head">
                                        <div>
                                            <h2 class="section-title">Service Traffic Distribution</h2>
                                            <p class="section-copy">Recent requests grouped by target microservice.</p>
                                        </div>
                                    </div>
                                    <div class="table-wrap">
                                        <table>
                                            <thead>
                                                <tr>
                                                    <th>Target Service</th>
                                                    <th>Requests</th>
                                                    <th>Errors</th>
                                                    <th>Avg Duration</th>
                                                </tr>
                                            </thead>
                                            <tbody>
            {{serviceRows}}
                                            </tbody>
                                        </table>
                                    </div>
                                </section>
                            </div>
                        </div>
                    </section>

                    <section id="logs" class="panel section">
                        <div class="panel-head">
                            <div>
                                <h2 class="section-title">Detailed Log Table</h2>
                                <p class="section-copy">Detailed dispatcher logs with timestamp, route, target service, status, duration, user, role and error message.</p>
                            </div>
                            <span class="hint">Required detailed log table section.</span>
                        </div>
                        <div class="table-wrap log-table-wrap">
                            <table>
                                <thead>
                                    <tr>
                                        <th>Timestamp (UTC)</th>
                                        <th>Method</th>
                                        <th>Path</th>
                                        <th>Target Service</th>
                                        <th>Status</th>
                                        <th>Duration</th>
                                        <th>User</th>
                                        <th>Role</th>
                                        <th>Error</th>
                                    </tr>
                                </thead>
                                <tbody>
            {{logRows}}
                                </tbody>
                            </table>
                        </div>
                    </section>

                    <section id="traffic" class="panel section">
                        <div class="panel-head">
                            <div>
                                <h2 class="section-title">Traffic Analytics</h2>
                                <p class="section-copy">Grafana time series for request intensity, service traffic, error classes and response latency.</p>
                            </div>
                            <span class="hint">Required graphical traffic monitoring section.</span>
                        </div>
                        <div class="traffic-panel">
                            <div class="analytics-grid">
                                <div class="analytics-card stat">
                                    <iframe src="{{Encode(BuildGrafanaPanelUrl(grafanaUrl, 1))}}" title="Grafana Request Rate"></iframe>
                                </div>
                                <div class="analytics-card stat">
                                    <iframe src="{{Encode(BuildGrafanaPanelUrl(grafanaUrl, 2))}}" title="Grafana Error Rate"></iframe>
                                </div>
                                <div class="analytics-card stat">
                                    <iframe src="{{Encode(BuildGrafanaPanelUrl(grafanaUrl, 3))}}" title="Grafana P95 Duration"></iframe>
                                </div>
                                <div class="analytics-card chart">
                                    <iframe src="{{Encode(BuildGrafanaPanelUrl(grafanaUrl, 4))}}" title="Grafana Requests by Target Service"></iframe>
                                </div>
                                <div class="analytics-card chart">
                                    <iframe src="{{Encode(BuildGrafanaPanelUrl(grafanaUrl, 5))}}" title="Grafana Error Volume by Status Class"></iframe>
                                </div>
                            </div>
                        </div>
                    </section>

                    <section id="load-tests" class="panel section">
                        <div class="panel-head">
                            <div>
                                <h2 class="section-title">Load Test Results</h2>
                                <p class="section-copy">Concurrent request scenarios shown in the UI and kept in sync only with `monitoring/load-tests/results-realistic.json`.</p>
                            </div>
                            <span class="hint">Source: latest realistic workflow load test result file.</span>
                        </div>
                        <div class="load-grid">
            {{loadTestCharts}}
                        </div>
                        <div class="table-wrap">
                            <table>
                                <thead>
                                    <tr>
                                        <th>Scenario</th>
                                        <th>Concurrent Requests</th>
                                        <th>Average Latency</th>
                                        <th>P95</th>
                                        <th>P99</th>
                                        <th>Error Rate</th>
                                        <th>Throughput</th>
                                        <th>Notes</th>
                                    </tr>
                                </thead>
                                <tbody>
            {{loadTestRows}}
                                </tbody>
                            </table>
                        </div>
                    </section>
                </div>
            </body>
            </html>
            """;
    }

    private static string BuildLogRows(List<RequestLog> logs)
    {
        var rows = new StringBuilder();

        foreach (var log in logs)
        {
            rows.AppendLine($"""
                <tr>
                    <td>{Encode(log.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss"))}</td>
                    <td><span class="method">{Encode(log.Method)}</span></td>
                    <td>{Encode(log.Path)}</td>
                    <td>{Encode(log.TargetService ?? "-")}</td>
                    <td><span class="status status-{log.StatusCode / 100}xx">{log.StatusCode}</span></td>
                    <td>{log.DurationMs:F2} ms</td>
                    <td>{Encode(log.UserId ?? "-")}</td>
                    <td>{Encode(log.UserRole ?? "-")}</td>
                    <td>{Encode(log.ErrorMessage ?? "-")}</td>
                </tr>
                """);
        }

        if (rows.Length == 0)
        {
            rows.AppendLine("""
                <tr>
                    <td colspan="9" class="empty">No request logs yet.</td>
                </tr>
                """);
        }

        return rows.ToString();
    }

    private static string BuildServiceRows(List<RequestLog> logs)
    {
        var rows = new StringBuilder();
        var groups = logs
            .Where(x => !string.IsNullOrWhiteSpace(x.TargetService))
            .GroupBy(x => x.TargetService!)
            .OrderByDescending(x => x.Count());

        foreach (var group in groups)
        {
            rows.AppendLine($"""
                <tr>
                    <td>{Encode(group.Key)}</td>
                    <td>{group.Count()}</td>
                    <td>{group.Count(x => x.StatusCode >= 400)}</td>
                    <td>{group.Average(x => x.DurationMs):F1} ms</td>
                </tr>
                """);
        }

        if (rows.Length == 0)
        {
            rows.AppendLine("""
                <tr>
                    <td colspan="4" class="empty">No target service traffic captured yet.</td>
                </tr>
                """);
        }

        return rows.ToString();
    }

    private static string BuildLoadTestCharts(List<LoadTestResult> results)
    {
        if (results.Count == 0)
            return """<article class="load-card"><h3>No Load Test Data</h3><p class="load-note">Run the realistic workflow load test and save the result file to see charts here.</p></article>""";

        var maxThroughput = Math.Max(1, results.Max(x => ParseMetricValue(x.ThroughputRps)));
        var maxLatency = Math.Max(1, results.Max(x => ParseMetricValue(x.P95LatencyMs)));
        var maxError = Math.Max(1, results.Max(x => ParseMetricValue(x.ErrorRatePercent)));
        var cards = new StringBuilder();

        foreach (var result in results)
        {
            var throughput = ParseMetricValue(result.ThroughputRps);
            var p95 = ParseMetricValue(result.P95LatencyMs);
            var errorRate = ParseMetricValue(result.ErrorRatePercent);

            cards.AppendLine($"""
                <article class="load-card">
                    <h3>{Encode(result.Scenario)} ({result.ConcurrentUsers})</h3>
                    <div class="metric-row">
                        <div class="metric-line"><span>Throughput</span><strong>{Encode(result.ThroughputRps)}</strong></div>
                        <div class="metric-bar"><div class="metric-fill" style="width:{ScalePercent(throughput, maxThroughput):F1}%"></div></div>
                    </div>
                    <div class="metric-row">
                        <div class="metric-line"><span>P95 Latency</span><strong>{Encode(result.P95LatencyMs)}</strong></div>
                        <div class="metric-bar"><div class="metric-fill warn" style="width:{ScalePercent(p95, maxLatency):F1}%"></div></div>
                    </div>
                    <div class="metric-row">
                        <div class="metric-line"><span>Error / Conflict Rate</span><strong>{Encode(result.ErrorRatePercent)}</strong></div>
                        <div class="metric-bar"><div class="metric-fill danger" style="width:{ScalePercent(errorRate, maxError):F1}%"></div></div>
                    </div>
                    <p class="load-note">{Encode(result.Notes ?? "-")}</p>
                </article>
                """);
        }

        return cards.ToString();
    }

    private static string BuildLoadTestRows(List<LoadTestResult> results)
    {
        var rows = new StringBuilder();

        foreach (var result in results)
        {
            rows.AppendLine($"""
                <tr>
                    <td>{Encode(result.Scenario)}</td>
                    <td>{result.ConcurrentUsers}</td>
                    <td>{Encode(result.AverageLatencyMs)}</td>
                    <td>{Encode(result.P95LatencyMs)}</td>
                    <td>{Encode(result.P99LatencyMs)}</td>
                    <td>{Encode(result.ErrorRatePercent)}</td>
                    <td>{Encode(result.ThroughputRps)}</td>
                    <td>{Encode(result.Notes ?? "-")}</td>
                </tr>
                """);
        }

        if (rows.Length == 0)
        {
            rows.AppendLine("""
                <tr>
                    <td colspan="8" class="empty">No realistic load test results found yet. Run the latest workflow test and write the measured values into monitoring/load-tests/results-realistic.json.</td>
                </tr>
                """);
        }

        return rows.ToString();
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static double ParseMetricValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        var filtered = new string(value.Where(ch => char.IsDigit(ch) || ch is ',' or '.').ToArray());
        if (string.IsNullOrWhiteSpace(filtered))
            return 0;

        var lastComma = filtered.LastIndexOf(',');
        var lastDot = filtered.LastIndexOf('.');
        string normalized;

        if (lastComma >= 0 && lastDot >= 0)
        {
            normalized = lastComma > lastDot
                ? filtered.Replace(".", string.Empty).Replace(',', '.')
                : filtered.Replace(",", string.Empty);
        }
        else if (lastComma >= 0)
        {
            normalized = filtered.Replace(',', '.');
        }
        else
        {
            normalized = filtered;
        }

        return double.TryParse(normalized, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }

    private static double ScalePercent(double value, double max) => max <= 0 ? 0 : Math.Clamp(value / max * 100, 0, 100);

    private static string BuildGrafanaPanelUrl(string grafanaBaseUrl, int panelId)
        => $"{grafanaBaseUrl.TrimEnd('/')}/d-solo/dispatcher-observability/dispatcher-observability?orgId=1&panelId={panelId}&from=now-15m&to=now&theme=dark";

    private static string BuildDashboardLoginHtml(string target)
    {
        var encodedTarget = Encode(target);

        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>Dispatcher Admin Login</title>
                <style>
                    body {
                        margin: 0;
                        min-height: 100vh;
                        display: grid;
                        place-items: center;
                        font-family: "Segoe UI", Tahoma, sans-serif;
                        background: linear-gradient(180deg, #f8f2e8 0%, #f5efe2 100%);
                        color: #1e1b18;
                    }
                    .card {
                        width: min(460px, calc(100% - 32px));
                        background: #fffdf8;
                        border: 1px solid #d8cfc3;
                        border-radius: 24px;
                        box-shadow: 0 18px 40px rgba(56, 44, 30, 0.12);
                        padding: 28px;
                    }
                    h1 { margin: 0 0 10px; font-size: 2rem; }
                    p { color: #6f655c; line-height: 1.55; }
                    label { display: block; margin-top: 14px; margin-bottom: 6px; font-weight: 600; }
                    input {
                        width: 100%;
                        padding: 12px 14px;
                        border-radius: 14px;
                        border: 1px solid #d8cfc3;
                        font: inherit;
                        box-sizing: border-box;
                    }
                    button {
                        margin-top: 18px;
                        width: 100%;
                        border: 0;
                        border-radius: 14px;
                        padding: 12px 14px;
                        font: inherit;
                        font-weight: 700;
                        background: #0f766e;
                        color: white;
                        cursor: pointer;
                    }
                    .hint { font-size: 0.92rem; margin-top: 14px; }
                    .error { margin-top: 14px; color: #b42318; font-weight: 600; min-height: 24px; }
                    .demo {
                        margin-top: 10px;
                        padding: 12px;
                        background: #f7f1e6;
                        border-radius: 14px;
                        font-size: 0.92rem;
                    }
                </style>
            </head>
            <body>
                <main class="card">
                    <h1>Admin Login</h1>
                    <p>JSON log export is protected. Sign in as an admin and the panel will open the requested JSON output automatically.</p>
                    <form id="login-form">
                        <label for="email">Email</label>
                        <input id="email" name="email" type="email" value="admin@system.local" required />

                        <label for="password">Password</label>
                        <input id="password" name="password" type="password" value="Admin123!" required />

                        <button type="submit">Sign In And Continue</button>
                    </form>
                    <div class="demo">Default seeded admin: <strong>admin@system.local</strong> / <strong>Admin123!</strong></div>
                    <p class="hint">Target after login: <code>{{encodedTarget}}</code></p>
                    <div class="error" id="error"></div>
                </main>

                <script>
                    const target = {{JsonSerializer.Serialize(target)}};
                    const form = document.getElementById('login-form');
                    const errorBox = document.getElementById('error');

                    form.addEventListener('submit', async (event) => {
                        event.preventDefault();
                        errorBox.textContent = '';

                        const email = document.getElementById('email').value;
                        const password = document.getElementById('password').value;

                        try {
                            const response = await fetch('/api/auth/login', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({ email, password })
                            });

                            if (!response.ok) {
                                errorBox.textContent = 'Login failed. Please verify admin credentials.';
                                return;
                            }

                            const payload = await response.json();
                            if (!payload.token) {
                                errorBox.textContent = 'Login succeeded but no token was returned.';
                                return;
                            }

                            sessionStorage.setItem('dispatcherAdminToken', payload.token);
                            window.location.href = target;
                        } catch (error) {
                            errorBox.textContent = 'Unexpected error while signing in.';
                        }
                    });
                </script>
            </body>
            </html>
            """;
    }

    private static string BuildDashboardLogsJsonHtml(int count)
    {
        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>Dispatcher Logs JSON</title>
                <style>
                    body {
                        margin: 0;
                        font-family: Consolas, "Courier New", monospace;
                        background: #111827;
                        color: #e5e7eb;
                    }
                    header {
                        padding: 18px 24px;
                        border-bottom: 1px solid #374151;
                        display: flex;
                        justify-content: space-between;
                        gap: 16px;
                        align-items: center;
                        flex-wrap: wrap;
                    }
                    a {
                        color: #67e8f9;
                        text-decoration: none;
                    }
                    pre {
                        margin: 0;
                        padding: 24px;
                        white-space: pre-wrap;
                        word-break: break-word;
                        line-height: 1.55;
                    }
                    .error {
                        color: #fca5a5;
                    }
                </style>
            </head>
            <body>
                <header>
                    <strong>Dispatcher Logs JSON (count={{count}})</strong>
                    <a href="/dashboard">Back to dashboard</a>
                </header>
                <pre id="output">Loading protected JSON logs...</pre>

                <script>
                    const token = sessionStorage.getItem('dispatcherAdminToken');
                    const output = document.getElementById('output');

                    if (!token) {
                        window.location.href = '/dashboard/login?target=' + encodeURIComponent('/dashboard/json/logs?count={{count}}');
                    } else {
                        fetch('/api/dispatcher/logs?count={{count}}', {
                            headers: {
                                'Authorization': 'Bearer ' + token
                            }
                        }).then(async (response) => {
                            if (!response.ok) {
                                output.className = 'error';
                                output.textContent = 'Could not load protected log JSON. Please sign in again as admin.';
                                sessionStorage.removeItem('dispatcherAdminToken');
                                return;
                            }

                            const data = await response.json();
                            output.textContent = JSON.stringify(data, null, 2);
                        }).catch(() => {
                            output.className = 'error';
                            output.textContent = 'Unexpected error while loading protected JSON logs.';
                        });
                    }
                </script>
            </body>
            </html>
            """;
    }

    private sealed class LoadTestResult
    {
        public string Scenario { get; set; } = default!;
        public int ConcurrentUsers { get; set; }
        public string AverageLatencyMs { get; set; } = default!;
        public string P95LatencyMs { get; set; } = default!;
        public string P99LatencyMs { get; set; } = default!;
        public string ErrorRatePercent { get; set; } = default!;
        public string ThroughputRps { get; set; } = default!;
        public string? Notes { get; set; }
    }
}
