using System.Runtime.CompilerServices;
using TeamTactics.Application.Common.Options;
using TeamTactics.Infrastructure.Tokens;

namespace TeamTactics.Api.Configurations
{
    public static class OptionSetup
    {
        public static void SetupOptions(this WebApplicationBuilder builder)
        {
            builder.Services.AddOptions<PasswordSecurityOptions>()
                .Bind(builder.Configuration.GetSection("PasswordSecurity"))
                .ValidateDataAnnotations();
            builder.Services.AddOptions<JwtOptions>()
                .Bind(builder.Configuration.GetSection("JWT"))
                .ValidateDataAnnotations();
        }
    }
}
