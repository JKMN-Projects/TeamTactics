using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Competitions;
using TeamTactics.Application.Players;
using TeamTactics.Application.Teams;
using TeamTactics.Application.Tournaments;
using TeamTactics.Application.Scraper;
using TeamTactics.Application.Users;
using TeamTactics.Infrastructure.Database.Repositories;
using TeamTactics.Infrastructure.Database.TypeHandlers;
using TeamTactics.Infrastructure.Database.Scraper;
using TeamTactics.Infrastructure.Hashing;
using TeamTactics.Infrastructure.Tokens;
using TeamTactics.Application.Bulletins;
using TeamTactics.Application.Points;

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

            // Register Dapper TypeHandlers
            Dapper.SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            Dapper.SqlMapper.AddTypeHandler(new DateOnlyNullableTypeHandler());

            services.AddScoped<IDbConnection>(sp => {
                var connection = new Npgsql.NpgsqlConnection(configuration.GetConnectionString("Postgres"));
                return connection;
            });

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<ICompetitionRepository, CompetitionRepository>();
            services.AddScoped<ITournamentRepository, TournamentRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IBulletinRepository, BulletinRepository>();
            services.AddScoped<IPointsRepository, PointRepository>();

            services.AddScoped<IScraperRepository,ScraperRepository>();
            return services;
        }
    }
}
