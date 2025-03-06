using Microsoft.Extensions.DependencyInjection;
using TeamTactics.Application.Players;
using TeamTactics.Application.Users;

namespace TeamTactics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Managers
        services.AddScoped<UserManager>();
        services.AddScoped<PlayerManager>();
        services.AddScoped<TeamManager>();
        services.AddScoped<CompetitionManager>();
        services.AddScoped<TournamentManager>();
        // Validators
        services.AddSingleton<PasswordValidator>();

        return services;
    }
}
