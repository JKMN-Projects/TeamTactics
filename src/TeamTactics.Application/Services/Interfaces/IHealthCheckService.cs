using TeamTactics.Application.Models;

namespace TeamTactics.Application.Services.Interfaces;

public interface IHealthCheckService
{
    Task<HealthStatus> CheckApplicationHealth();
}
