using Microsoft.Extensions.DependencyInjection;
using TeamTactics.Application.Services.Implementation;
using TeamTactics.Application.Services.Interfaces;

namespace TeamTactics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHealthCheckService, HealthCheckService>();
        return services;
    }
}
