using Microsoft.Extensions.DependencyInjection;

namespace TeamTactics.Infrastructure.IntegrationTests.Configuration
{
    public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private IServiceScope? _scope;

        protected TestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            CreateNewScope();
        }

        private void CreateNewScope()
        {
            _scope?.Dispose();
            _scope = _factory.Services.CreateScope();
        }

        protected T GetService<T>()
            where T : class
        {
            if (_scope == null)
            {
                throw new InvalidOperationException("Service provider is not initialized.");
            }
            return _scope.ServiceProvider.GetRequiredService<T>();
        }
    }
}
