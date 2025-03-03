using Microsoft.Extensions.DependencyInjection;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Players;
using TeamTactics.Application.Users;
using TeamTactics.Infrastructure.Database.Repositories;
using TeamTactics.Infrastructure.Hashing;

namespace TeamTactics.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IHashingService, Rfc2898HashingService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            return services;
        }
    }
}
