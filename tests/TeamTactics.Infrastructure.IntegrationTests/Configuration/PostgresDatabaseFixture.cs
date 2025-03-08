using DbMigrator;
using TeamTactics.Infrastructure.Database.TypeHandlers;
using Testcontainers.PostgreSql;

namespace TeamTactics.Infrastructure.IntegrationTests.Configuration
{
    public class PostgresDatabaseFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("TeamTactics-IntegrationTests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        public string ConnectionString => postgreSqlContainer.GetConnectionString();

        public async Task InitializeAsync()
        {
            await postgreSqlContainer.StartAsync();

            Dapper.SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            Dapper.SqlMapper.AddTypeHandler(new DateOnlyNullableTypeHandler());

            DatabaseMigrator.MigrateDatabase(ConnectionString);
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await postgreSqlContainer.StopAsync();
            await postgreSqlContainer.DisposeAsync();
        }
    }
}
