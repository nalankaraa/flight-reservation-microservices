using Dispatcher.Api.Observability;
using FluentAssertions;

namespace Dispatcher.Tests;

public class DispatcherMetricsStoreTests
{
    [Fact]
    public void RenderPrometheus_Should_Expose_Request_And_Duration_Metrics()
    {
        var store = new DispatcherMetricsStore();

        store.RecordRequest("/api/flights", "GET", 200, 24.5, "FlightService");
        store.RecordRequest("/api/flights", "GET", 404, 80.1, "FlightService");

        var metrics = store.RenderPrometheus();

        metrics.Should().Contain("dispatcher_requests_total");
        metrics.Should().Contain("path=\"/api/flights\"");
        metrics.Should().Contain("status_code=\"200\"");
        metrics.Should().Contain("status_code=\"404\"");
        metrics.Should().Contain("dispatcher_request_duration_ms_bucket");
        metrics.Should().Contain("target_service=\"FlightService\"");
    }
}
