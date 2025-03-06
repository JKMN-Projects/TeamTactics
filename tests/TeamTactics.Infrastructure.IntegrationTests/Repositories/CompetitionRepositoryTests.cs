
using Dapper;
using System.Data;
using TeamTactics.Domain.Competitions;
using TeamTactics.Fixtures;
using TeamTactics.Infrastructure.Database.Repositories;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories
{
    public abstract class CompetitionRepositoryTests : TestBase
    {
        private readonly CompetitionRepository _sut;
        private readonly IDbConnection _dbConnection;

        protected CompetitionRepositoryTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _dbConnection = GetService<IDbConnection>();
            _sut = new CompetitionRepository(_dbConnection);
        }

        public sealed class FindAllAsync : CompetitionRepositoryTests
        {
            public FindAllAsync(CustomWebApplicationFactory factory) : base(factory)
            {
            }

            private IEnumerable<Competition> SeedCompetitions()
            {
                List<Competition> seededCompetitions = [
                    new Competition("Premier League", new DateOnly(2021, 8, 1), new DateOnly(2022, 5, 1)),
                    new Competition("La Liga", new DateOnly(2021, 8, 1), new DateOnly(2022, 5, 1)),
                    new Competition("Serie A", new DateOnly(2021, 8, 1), new DateOnly(2022, 5, 1))
                ];

                List<string> valueClauses = new List<string>();
                var parameters = new DynamicParameters();
                for (int i = 0; i < seededCompetitions.Count; i++)
                {
                    valueClauses.Add($"(@ExternalId{i}, @Name{i}, @StartDate{i}, @EndDate{i})");
                    parameters.Add($"ExternalId{i}", Guid.NewGuid().ToString().Substring(0, 10));
                    parameters.Add($"Name{i}", seededCompetitions[i].Name);
                    parameters.Add($"StartDate{i}", seededCompetitions[i].StartDate.ToDateTime(TimeOnly.MinValue));
                    parameters.Add($"EndDate{i}", seededCompetitions[i].EndDate.ToDateTime(TimeOnly.MinValue));
                }
                string sql = $@"
                    INSERT INTO team_tactics.competition (external_id, name, start_date, end_date) 
                        VALUES {string.Join(", ", valueClauses)}";

                _dbConnection.Query(sql, parameters);
                return seededCompetitions;
            }


            [Fact]
            public async Task Should_ReturnAllCompetitions()
            {
                // Arrange
                var expectedCompetitions = SeedCompetitions();

                // Act
                var competitions = await _sut.FindAllAsync();

                // Assert
                var competitionNames = competitions.Select(c => c.Name);
                foreach (var expectedCompetition in expectedCompetitions)
                {
                    Assert.Contains(expectedCompetition.Name, competitionNames);
                }
            }
        }

        public sealed class FindByIdAsync : CompetitionRepositoryTests
        {
            public FindByIdAsync(CustomWebApplicationFactory factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_ReturnCompetition_When_CompetitionExists()
            {
                // Arrange
                var expectedCompetition = new CompetitionFaker("Premier League")
                    .Generate();
                int competitionId = await _dbConnection.SeedCompetitonAsync(expectedCompetition);

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
