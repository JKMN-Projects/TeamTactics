
using TeamTactics.Application.Models;
using TeamTactics.Application.Services.Interfaces;

namespace TeamTactics.Application.Services.Implementation;

internal class HealthCheckService : IHealthCheckService
{
    public Task<HealthStatus> CheckApplicationHealth()
    {
        throw new NotImplementedException();
    }
}
