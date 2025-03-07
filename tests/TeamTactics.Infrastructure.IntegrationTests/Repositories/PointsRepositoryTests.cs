using TeamTactics.Domain.Points;
using TeamTactics.Infrastructure.Database.Repositories;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories;

public abstract class PointsRepositoryTests : TestBase
{
    private readonly PointRepository _pointsRepository;

    protected PointsRepositoryTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _pointsRepository = new PointRepository(_dbConnection);
    }

    
    public sealed class FindAllActiveAsync : PointsRepositoryTests
    {
        public FindAllActiveAsync(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_ReturnActivePointsCategories()
        {
            // Arrange
            PointCategory pointCategory = new PointCategory(default, "Test", "Test", 1, false);
            var parameters = new DynamicParameters();
            parameters.Add("Name", pointCategory.Name);
            parameters.Add("Description", pointCategory.Description);
            parameters.Add("PointAmount", pointCategory.PointAmount);
            parameters.Add("Active", pointCategory.Active);
            string insertInactivePointCategorySql = @"
                INSERT INTO team_tactics.point_category (name, description, point_amount, active) 
                VALUES (@Name, @Description, @PointAmount, @Active)";
            await _dbConnection.ExecuteAsync(insertInactivePointCategorySql, parameters);

            // Act
            var actual = await _pointsRepository.FindAllActiveAsync();

            // Assert
            Assert.NotEmpty(actual);
            Assert.DoesNotContain(actual, x => x.Name == pointCategory.Name);
        }
    }
}