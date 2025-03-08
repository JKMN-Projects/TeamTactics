using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn.Graph;

namespace TeamTactics.Infrastructure.IntegrationTests.Configuration
{
    public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>
    {   
        private readonly CustomWebApplicationFactory _factory;
        private IServiceScope? _scope;
        protected IDbConnection _dbConnection { get; private set; } = default!;

        protected TestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            CreateNewScope();
        }

        private void CreateNewScope()
        {
            _scope?.Dispose();
            _scope = _factory.Services.CreateScope();
            _dbConnection = GetService<IDbConnection>();
        }

        protected void RefreshScope()
        {
            CreateNewScope();
        }

        protected async Task ResetDatabaseAsync()
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();
            var respawner = await Respawner.CreateAsync((NpgsqlConnection)_dbConnection, new RespawnerOptions
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

            await respawner.ResetAsync((NpgsqlConnection)_dbConnection); 
        }

        protected T GetService<T>()
            where T : class => _scope == null
                ? throw new InvalidOperationException("Service provider is not initialized.")
                : _scope.ServiceProvider.GetRequiredService<T>();
    }
}
