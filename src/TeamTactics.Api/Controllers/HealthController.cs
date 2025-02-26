using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamTactics.Application.Services.Interfaces;

namespace TeamTactics.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;
    public HealthController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }
    /// <summary>
    /// Endpoint to check api is reachable
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public IActionResult Get()
    {
        return Ok();
    }
    /// <summary>
    /// Endpoint to check health through the Application
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("Ready")]
    public async Task<IActionResult> Ready()
    {
        await _healthCheckService.CheckApplicationHealth();
        throw new NotImplementedException();
    }

}
