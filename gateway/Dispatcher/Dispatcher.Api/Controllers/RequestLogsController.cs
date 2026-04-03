using Dispatcher.Application.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dispatcher.Api.Controllers;

[ApiController]
[Route("api/dispatcher/logs")]
[Authorize(Roles = "Admin")]
public class RequestLogsController : ControllerBase
{
    private readonly IRequestLogRepository _requestLogRepository;

    public RequestLogsController(IRequestLogRepository requestLogRepository)
    {
        _requestLogRepository = requestLogRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 100)
    {
        var normalizedCount = Math.Clamp(count, 1, 500);
        var logs = await _requestLogRepository.GetRecentAsync(normalizedCount);
        return Ok(logs);
    }
}