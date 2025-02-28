using Microsoft.Extensions.DependencyInjection;
using TeamTactics.Application.Users;

namespace TeamTactics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        // Managers
        services.AddScoped<UserManager>();

        // Validators
        services.AddSingleton<PasswordValidator>();

        return services;
    }
}
