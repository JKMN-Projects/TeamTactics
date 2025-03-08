using Npgsql;
using Respawn.Graph;

namespace TeamTactics.Infrastructure.IntegrationTests.Configuration
{
    public abstract class RepositoryTestBase : IClassFixture<PostgresDatabaseFixture>, IAsyncLifetime
    {   
        protected NpgsqlConnection _dbConnection { get; private set; } = default!;

        private Respawner _respawner = null!;

        protected RepositoryTestBase(PostgresDatabaseFixture dbFixture)
        {
            _dbConnection = new NpgsqlConnection(dbFixture.ConnectionString);
        }

        protected async Task ResetDatabaseAsync()
        {
            await _respawner.ResetAsync(_dbConnection); 
        }

        public virtual async Task InitializeAsync()
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();
            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
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
