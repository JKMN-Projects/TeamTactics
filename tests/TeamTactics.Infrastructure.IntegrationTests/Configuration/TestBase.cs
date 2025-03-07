using Microsoft.Extensions.DependencyInjection;

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
            OnRefreshScope();
        }
        
        protected virtual void OnRefreshScope() {}

        protected T GetService<T>()
            where T : class => _scope == null
                ? throw new InvalidOperationException("Service provider is not initialized.")
                : _scope.ServiceProvider.GetRequiredService<T>();
    }
}
