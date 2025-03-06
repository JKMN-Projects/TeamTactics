using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace TeamTactics.Infrastructure.IntegrationTests.Configuration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("TeamTactics-IntegrationTests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            string connString = postgreSqlContainer.GetConnectionString();
            builder.UseSetting(
                "Connectionstrings:Postgres",
                connString + ";Include Error Detail=true");
        }


        public async Task InitializeAsync()
        {
            await postgreSqlContainer.StartAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await postgreSqlContainer.StopAsync();
        }
    }
}
