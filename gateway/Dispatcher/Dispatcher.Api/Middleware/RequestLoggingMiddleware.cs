using System.Diagnostics;
using Dispatcher.Application.Logging;
using Dispatcher.Domain.Logging;

namespace Dispatcher.Api.Middleware;

public class RequestLoggingMiddleware
{
	private readonly RequestDelegate _next;

	public RequestLoggingMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context, IRequestLogRepository logRepository)
	{
		var stopwatch = Stopwatch.StartNew();

		await _next(context);

		stopwatch.Stop();

		var log = new RequestLog
		{
			Path = context.Request.Path.Value ?? string.Empty,
			Method = context.Request.Method,
			StatusCode = context.Response.StatusCode,
			DurationMs = stopwatch.Elapsed.TotalMilliseconds,
			TimestampUtc = DateTime.UtcNow
		};

		await logRepository.AddAsync(log);
	}
}