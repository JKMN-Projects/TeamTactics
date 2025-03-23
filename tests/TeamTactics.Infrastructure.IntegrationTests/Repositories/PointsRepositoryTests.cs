using TeamTactics.Domain.Points;
using TeamTactics.Infrastructure.Database.Repositories;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories;

public abstract class PointsRepositoryTests : RepositoryTestBase, IAsyncLifetime
{
    private readonly PointRepository _pointsRepository;

    protected PointsRepositoryTests(PostgresDatabaseFixture factory) : base(factory)
    {
        _pointsRepository = new PointRepository(DbConnection);
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await ResetDatabaseAsync();
    }

    public override Task InitializeAsync() => base.InitializeAsync();

    public sealed class FindAllActiveAsync : PointsRepositoryTests
    {
        public FindAllActiveAsync(PostgresDatabaseFixture factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_ReturnActivePointsCategories()
        {
            // Arrange
            PointCategory inactivePointCategory = new PointCategory(default, "Test", "Test", 1, false);
            var parameters = new DynamicParameters();
            parameters.Add("Name", inactivePointCategory.Name);
            parameters.Add("Description", inactivePointCategory.Description);
            parameters.Add("PointAmount", inactivePointCategory.PointAmount);
            parameters.Add("Active", inactivePointCategory.Active);
            string insertInactivePointCategorySql = @"
                INSERT INTO team_tactics.point_category (name, description, point_amount, active) 
                VALUES (@Name, @Description, @PointAmount, @Active)";
            await DbConnection.ExecuteAsync(insertInactivePointCategorySql, parameters);

            // Act
            var actual = await _pointsRepository.FindAllActiveAsync();

            // Assert
            Assert.NotEmpty(actual);
            Assert.DoesNotContain(actual, x => x.Name == inactivePointCategory.Name);
        }
    }
}