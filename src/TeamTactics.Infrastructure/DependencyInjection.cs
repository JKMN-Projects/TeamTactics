using Microsoft.Extensions.DependencyInjection;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Infrastructure.Hashing;

namespace TeamTactics.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IHashingService, Rfc2898HashingService>();
            return services;
        }
    }
}
