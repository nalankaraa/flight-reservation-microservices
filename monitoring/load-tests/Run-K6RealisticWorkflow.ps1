param(
    [string]$Duration = "15s",
    [string]$Image = "grafana/k6:0.49.0"
)

$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$rawDirectory = Join-Path $scriptDirectory "raw-realistic"

if (-not (Test-Path $rawDirectory)) {
    New-Item -ItemType Directory -Path $rawDirectory | Out-Null
}

$scenarios = @(
    @{ Name = "Realistic-50"; ConcurrentUsers = 50 },
    @{ Name = "Realistic-100"; ConcurrentUsers = 100 },
    @{ Name = "Realistic-200"; ConcurrentUsers = 200 },
    @{ Name = "Realistic-500"; ConcurrentUsers = 500 }
)

foreach ($scenario in $scenarios) {
    Write-Host ("Running realistic workflow scenario {0} with {1} VUs for {2}..." -f $scenario.Name, $scenario.ConcurrentUsers, $Duration)

    docker run --rm `
        -v "${scriptDirectory}:/scripts" `
        $Image run `
        --vus $scenario.ConcurrentUsers `
        --duration $Duration `
        --summary-trend-stats "avg,min,med,max,p(90),p(95),p(99)" `
        --summary-export "/scripts/raw-realistic/summary-$($scenario.ConcurrentUsers).json" `
        -e BASE_URL=http://host.docker.internal:8080 `
        /scripts/dispatcher-realistic-workflow.js

    Start-Sleep -Seconds 5
}

$results = foreach ($scenario in $scenarios) {
    $summary = Get-Content -Raw -Path (Join-Path $rawDirectory ("summary-{0}.json" -f $scenario.ConcurrentUsers)) | ConvertFrom-Json
    $businessConflicts = if ($summary.metrics.business_conflicts) { [int]$summary.metrics.business_conflicts.count } else { 0 }
    $successfulReservations = if ($summary.metrics.successful_reservations) { [int]$summary.metrics.successful_reservations.count } else { 0 }
    $successfulCheckouts = if ($summary.metrics.successful_checkouts) { [int]$summary.metrics.successful_checkouts.count } else { 0 }
    $systemFailureRate = if ($summary.metrics.system_failure_rate) { [math]::Round($summary.metrics.system_failure_rate.rate * 100, 2) } else { 0 }

    [PSCustomObject]@{
        scenario = $scenario.Name
        concurrentUsers = $scenario.ConcurrentUsers
        averageLatencyMs = ("{0:N2} ms" -f $summary.metrics.http_req_duration.avg)
        p95LatencyMs = ("{0:N2} ms" -f $summary.metrics.http_req_duration.'p(95)')
        p99LatencyMs = ("{0:N2} ms" -f $summary.metrics.http_req_duration.'p(99)')
        errorRatePercent = ("{0:N2}%" -f ($summary.metrics.http_req_failed.value * 100))
        throughputRps = ("{0:N2} req/s" -f $summary.metrics.http_reqs.rate)
        notes = "weighted traffic: 50% search, 20% detail, 15% availability, 10% reservation, 5% checkout; conflicts=$businessConflicts, reservations=$successfulReservations, checkouts=$successfulCheckouts, system5xx=$systemFailureRate%"
    }
}

$resultsPath = Join-Path $scriptDirectory "results-realistic.json"
$results | ConvertTo-Json -Depth 5 | Set-Content -Path $resultsPath -Encoding UTF8

Write-Host ""
Write-Host "Realistic workflow results written to:"
Write-Host " - $resultsPath"
Write-Host " - $rawDirectory"
