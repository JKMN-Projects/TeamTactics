using Microsoft.Extensions.DependencyInjection;
using TeamTactics.Application.Competitions;
using TeamTactics.Application.Players;
using TeamTactics.Application.Points;
using TeamTactics.Application.Teams;
using TeamTactics.Application.Tournaments;
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
        services.AddScoped<PointsManager>();
        // Validators
        services.AddSingleton<PasswordValidator>();

        return services;
    }
}
