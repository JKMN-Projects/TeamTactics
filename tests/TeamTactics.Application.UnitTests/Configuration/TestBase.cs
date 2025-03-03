
namespace TeamTactics.Application.UnitTests.Configuration
{
    public abstract class TestBase
    {
        protected readonly Faker _faker;

        protected TestBase()
        {
            _faker = new Faker();
        }
    }
}
