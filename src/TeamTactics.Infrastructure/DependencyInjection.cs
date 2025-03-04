using Microsoft.Extensions.DependencyInjection;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Players;
using TeamTactics.Application.Users;
using TeamTactics.Infrastructure.Database.Repositories;
using TeamTactics.Infrastructure.Hashing;
using TeamTactics.Infrastructure.Tokens;

namespace TeamTactics.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IHashingService, Rfc2898HashingService>();
            services.AddSingleton<IAuthTokenProvider, JwtTokenProvider>();
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            return services;
        }
    }
}
