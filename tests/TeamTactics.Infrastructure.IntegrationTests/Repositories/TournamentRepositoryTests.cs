
using System.Data;
using TeamTactics.Domain.Tournaments;
using TeamTactics.Infrastructure.Database.Repositories;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories;

public abstract class TournamentRepositoryTests : TestBase
{
    private readonly IDbConnection _dbConnection;
    private readonly TournamentRepository _sut;

    protected TournamentRepositoryTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _dbConnection = GetService<IDbConnection>();
        _sut = new TournamentRepository(_dbConnection);
    }

    public sealed class InsertAsync : TournamentRepositoryTests
    {
        public InsertAsync(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_AddTournamentToDb_When_TournamentIsInserted()
        {
            // Arrange
            int userId = await _dbConnection.SeedUserAsync();
            int competitionId = await _dbConnection.SeedCompetitonAsync();
            var tournamentToInsert = new Tournament("Test Tournament", userId, competitionId, description: "A Tournament description");

            // Act
            int tournamentId = await _sut.InsertAsync(tournamentToInsert);

            // Assert
            var tournament = await _sut.FindByIdAsync(tournamentId);
            Assert.NotNull(tournament);
            Assert.Equal(tournamentToInsert.Name, tournament.Name);
            Assert.Equal(tournamentToInsert.Description, tournament.Description);
            Assert.Equal(tournamentToInsert.CreatedByUserId, tournament.CreatedByUserId);
            Assert.Equal(tournamentToInsert.CompetitionId, tournament.CompetitionId);
        }
    }
}
