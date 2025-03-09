using TeamTactics.Domain.Competitions;
using TeamTactics.Infrastructure.Database.Repositories;
using TeamTactics.Infrastructure.IntegrationTests.Seeding;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories
{
    public abstract class CompetitionRepositoryTests : RepositoryTestBase
    {
        private readonly CompetitionRepository _sut;
        private readonly DataSeeder _dataSeeder;

        protected CompetitionRepositoryTests(PostgresDatabaseFixture factory) : base(factory)
        {
            _sut = new CompetitionRepository(_dbConnection);
            _dataSeeder = new DataSeeder(_dbConnection);
        }

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync();
            await ResetDatabaseAsync();
        }

        public override Task InitializeAsync() => base.InitializeAsync();

        public sealed class FindAllAsync : CompetitionRepositoryTests
        {
            public FindAllAsync(PostgresDatabaseFixture factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_ReturnAllCompetitions()
            {
                // Arrange
                List<Competition> seededCompetitions = [
                    new Competition("Premier League", new DateOnly(2021, 8, 1), new DateOnly(2022, 5, 1)),
                    new Competition("La Liga", new DateOnly(2021, 8, 1), new DateOnly(2022, 5, 1)),
                    new Competition("Serie A", new DateOnly(2021, 8, 1), new DateOnly(2022, 5, 1))
                ];
                foreach (var competition in seededCompetitions)
                {
                    await _dataSeeder.SeedCompetitionAsync(competition);
                }

                // Act
                var competitions = await _sut.FindAllAsync();

                // Assert
                var competitionNames = competitions.Select(c => c.Name);
                foreach (var expectedCompetition in seededCompetitions)
                {
                    Assert.Contains(expectedCompetition.Name, competitionNames);
                }
            }
        }

        public sealed class FindByIdAsync : CompetitionRepositoryTests
        {
            public FindByIdAsync(PostgresDatabaseFixture factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_ReturnCompetition_When_CompetitionExists()
            {
                // Arrange
                var expectedCompetition = new CompetitionFaker("Premier League")
                    .Generate();
                int competitionId = await _dataSeeder.SeedCompetitionAsync(expectedCompetition);

                // Act
                var competition = await _sut.FindByIdAsync(competitionId);

                // Assert
                Assert.NotNull(competition);
                Assert.NotEqual((int)default, competition.Id);
                Assert.Equal(expectedCompetition.Name, competition.Name);
                Assert.Equal(expectedCompetition.StartDate, competition.StartDate);
                Assert.Equal(expectedCompetition.EndDate, competition.EndDate);
            }

            [Fact]
            public async Task Should_ReturnNull_When_CompetitionDoesNotExist()
            {
                // Arrange
                int competitionId = 99;

                // Act
                var competition = await _sut.FindByIdAsync(competitionId);

                // Assert
                Assert.Null(competition);
            }
        }
    }
}
