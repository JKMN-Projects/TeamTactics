using Npgsql;
using Respawn.Graph;

namespace TeamTactics.Infrastructure.IntegrationTests.Configuration
{
    public abstract class RepositoryTestBase : IClassFixture<PostgresDatabaseFixture>, IAsyncLifetime
    {   
        protected NpgsqlConnection DbConnection { get; private set; } = default!;

        private Respawner _respawner = null!;

        protected RepositoryTestBase(PostgresDatabaseFixture dbFixture)
        {
            DbConnection = new NpgsqlConnection(dbFixture.ConnectionString);
        }

        protected async Task ResetDatabaseAsync()
        {
            await _respawner.ResetAsync(DbConnection); 
        }

        public virtual async Task InitializeAsync()
        {
            if (DbConnection.State != ConnectionState.Open)
                DbConnection.Open();
            _respawner = await Respawner.CreateAsync(DbConnection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[]
                {
                    "team_tactics"
                },
                TablesToIgnore = [
                    new Table("team_tactics", "player_position"),
                    new Table("team_tactics", "point_category"),
                ]
            });
        }

        public virtual Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
