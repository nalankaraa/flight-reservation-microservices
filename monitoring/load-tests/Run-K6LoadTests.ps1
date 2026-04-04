param(
    [string]$BaseUrl = "http://localhost:8080",
    [string]$ContainerBaseUrl = "http://host.docker.internal:8080",
    [string]$Duration = "15s",
    [string]$Image = "grafana/k6:0.49.0"
)

$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$rawDirectory = Join-Path $scriptDirectory "raw"

if (-not (Test-Path $rawDirectory)) {
    New-Item -ItemType Directory -Path $rawDirectory | Out-Null
}

$scenarios = @(
    @{ Name = "Warm-up"; ConcurrentUsers = 50 },
    @{ Name = "Peak"; ConcurrentUsers = 100 },
    @{ Name = "Stress"; ConcurrentUsers = 200 },
    @{ Name = "Burst"; ConcurrentUsers = 500 }
)

$loginResponse = Invoke-RestMethod `
    -Method Post `
    -Uri "$BaseUrl/api/auth/login" `
    -ContentType "application/json" `
    -Body '{"email":"admin@system.local","password":"Admin123!"}'

if (-not $loginResponse.token) {
    throw "Could not obtain admin token before running k6 scenarios."
}

$token = $loginResponse.token
$headers = @{
    Authorization = "Bearer $token"
}

$flights = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/flights" -Headers $headers
$flightId = $null

if ($flights -is [System.Array] -and $flights.Count -gt 0) {
    $flightId = $flights[0].id
}
elseif ($flights.id) {
    $flightId = $flights.id
}

if (-not $flightId) {
    $createdFlight = Invoke-RestMethod `
        -Method Post `
        -Uri "$BaseUrl/api/flights" `
        -Headers ($headers + @{ "Content-Type" = "application/json" }) `
        -Body (@{
            from = "IST-K6"
            to = "ESB-K6"
            departureTime = "2026-05-01T08:30:00Z"
            arrivalTime = "2026-05-01T09:45:00Z"
            price = 1499
            availableSeatCount = 120
        } | ConvertTo-Json)

    $flightId = $createdFlight.id
}

if (-not $flightId) {
    throw "Could not determine a flight id before running k6 scenarios."
}

foreach ($scenario in $scenarios) {
    $summaryFile = Join-Path $rawDirectory ("summary-{0}.json" -f $scenario.ConcurrentUsers)
    Write-Host ("Running k6 scenario {0} with {1} VUs for {2}..." -f $scenario.Name, $scenario.ConcurrentUsers, $Duration)

    docker run --rm `
        -v "${scriptDirectory}:/scripts" `
        $Image run `
        --vus $scenario.ConcurrentUsers `
        --duration $Duration `
        --summary-trend-stats "avg,min,med,max,p(90),p(95),p(99)" `
        --summary-export "/scripts/raw/summary-$($scenario.ConcurrentUsers).json" `
        -e BASE_URL=$ContainerBaseUrl `
        -e TOKEN=$token `
        -e FLIGHT_ID=$flightId `
        /scripts/dispatcher-load.js

    if (-not (Test-Path $summaryFile)) {
        throw "Summary file was not produced for $($scenario.ConcurrentUsers) VUs."
    }

    Start-Sleep -Seconds 5
}

$results = foreach ($scenario in $scenarios) {
    $summaryPath = Join-Path $rawDirectory ("summary-{0}.json" -f $scenario.ConcurrentUsers)
    $summary = Get-Content -Path $summaryPath -Raw | ConvertFrom-Json
    $durationValues = $summary.metrics.http_req_duration
    $failureValues = $summary.metrics.http_req_failed
    $requestValues = $summary.metrics.http_reqs

    [PSCustomObject]@{
        scenario = $scenario.Name
        concurrentUsers = $scenario.ConcurrentUsers
        averageLatencyMs = ("{0:N2} ms" -f $durationValues.avg)
        p95LatencyMs = ("{0:N2} ms" -f $durationValues.'p(95)')
        p99LatencyMs = ("{0:N2} ms" -f $durationValues.'p(99)')
        errorRatePercent = ("{0:N2}%" -f ($failureValues.value * 100))
        throughputRps = ("{0:N2} req/s" -f $requestValues.rate)
        notes = "k6 mixed dispatcher workload over $Duration against $BaseUrl"
    }
}

$resultsPath = Join-Path $scriptDirectory "results.json"
$results | ConvertTo-Json -Depth 5 | Set-Content -Path $resultsPath -Encoding UTF8

Write-Host ""
Write-Host "Load test results written to:"
Write-Host " - $resultsPath"
Write-Host " - $rawDirectory"
