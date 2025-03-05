namespace TeamTactics.Api.Configurations
{
    public static class CorsSetup
    {
        internal const string CORS_POLICY = "_allow";
        public static void SetupCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CORS_POLICY, builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }
    }
}
