using System.Data;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Teams;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;
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
            //var tournament = await _sut.FindByIdAsync(tournamentId);
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();
            var tournament = await _dbConnection.QuerySingleOrDefaultAsync<Tournament>($@"
    SELECT id, name, description, user_account_id as CreatedByUserId, competition_id as CompetitionId
    FROM team_tactics.user_tournament
    WHERE id = @Id", 
    new { Id = tournamentId });

            Assert.NotNull(tournament);
            Assert.Equal(tournamentToInsert.Name, tournament.Name);
            Assert.Equal(tournamentToInsert.Description, tournament.Description);
            Assert.Equal(tournamentToInsert.CreatedByUserId, tournament.CreatedByUserId);
            Assert.Equal(tournamentToInsert.CompetitionId, tournament.CompetitionId);
        }
    }

    public sealed class RemoveAsync : TournamentRepositoryTests
    {
        public RemoveAsync(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_DeleteTournamentFromDb_When_TournamentExists()
        {
            // Arrange
            int userId = await _dbConnection.SeedUserAsync();
            int competitionId = await _dbConnection.SeedCompetitonAsync();
            var tournamentToInsert = new Tournament("Test Tournament", userId, competitionId, description: "A Tournament description");
            int tournamentId = await _sut.InsertAsync(tournamentToInsert);

            // Act
            await _sut.RemoveAsync(tournamentId);

            // Assert
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();
            var tournament = await _dbConnection.QuerySingleOrDefaultAsync<Tournament>($@"
    SELECT id, name, description, user_account_id as CreatedByUserId, competition_id as CompetitionId
    FROM team_tactics.user_tournament
    WHERE id = @Id",
                new { Id = tournamentId });

            Assert.Null(tournament);
        }

        [Fact]
        public async Task Should_Throw_When_TournamentDoesNotExist()
        {
            // Arrange
            int tournamentId = 99;

            // Act
            Func<Task> act = async () => await _sut.RemoveAsync(tournamentId);

            // Assert
            var ex = await Assert.ThrowsAsync<EntityNotFoundException>(act);
            Assert.Equal(nameof(Tournament), ex.EntityType);
            Assert.Equal(tournamentId, ex.Key);
            Assert.Equal("Id", ex.KeyName);
        }

        [Fact]
        public async Task Should_DeleteEnrolledTeam_When_TournamentIsDeleted()
        {
            // Arrange
            int userId = await _dbConnection.SeedUserAsync();
            int competitionId = await _dbConnection.SeedCompetitonAsync();
            var tournamentToInsert = new Tournament("Test Tournament", userId, competitionId, description: "A Tournament description");
            int tournamentId = await _sut.InsertAsync(tournamentToInsert);
            var teamRepository = GetService<ITeamRepository>();

            List<Player> players = new PlayerFaker().Generate(11);
            foreach (var player in players)
            {
                int playerId = await _dbConnection.SeedPlayer(player);
                player.SetId(playerId);
            }

            List<Team> teams = new TeamFaker(players: players)
                .RuleFor(t => t.UserId, userId)
                .RuleFor(t => t.TournamentId, tournamentId)
                .Generate(3);

            foreach (var team in teams)
            {
                await teamRepository.InsertAsync(team);
            }

            // Act
            await _sut.RemoveAsync(tournamentId);

            // Assert
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            string sql = @"
    SELECT id
    FROM team_tactics.user_team
    WHERE user_tournament_id = @TournamentId";
            var parameters = new DynamicParameters();
            parameters.Add("TournamentId", tournamentId);

            var teamIds = await _dbConnection.QueryAsync<int>(sql, parameters);

            Assert.Empty(teamIds);
        }
    }
}
