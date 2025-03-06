using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.Common;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Competitions;
using TeamTactics.Application.Players;
using TeamTactics.Application.Users;
using TeamTactics.Infrastructure.Database.Repositories;
using TeamTactics.Infrastructure.Hashing;
using TeamTactics.Infrastructure.Tokens;

namespace TeamTactics.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IHashingService, Rfc2898HashingService>();
            services.AddSingleton<IAuthTokenProvider, JwtTokenProvider>();

            var connectString = configuration.GetConnectionString("Postgres");

            services.AddScoped<IDbConnection>(sp => {
                var connection = new Npgsql.NpgsqlConnection(configuration.GetConnectionString("Postgres"));
                return connection;
            });

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<ICompetitionRepository, CompetitionRepository>();
            return services;
        }
    }
}
