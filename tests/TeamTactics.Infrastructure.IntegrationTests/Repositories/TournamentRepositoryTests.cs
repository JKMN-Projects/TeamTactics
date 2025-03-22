using System.Data;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Teams;
using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Tournaments;
using TeamTactics.Domain.Users;
using TeamTactics.Infrastructure.Database.Repositories;
using TeamTactics.Infrastructure.IntegrationTests.Seeding;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories;

public abstract class TournamentRepositoryTests : RepositoryTestBase, IAsyncLifetime
{
    private readonly TournamentRepository _sut;
    private readonly DataSeeder _dataSeeder;

    protected TournamentRepositoryTests(PostgresDatabaseFixture factory) : base(factory)
    {
        _sut = new TournamentRepository(_dbConnection);
        _dataSeeder = new DataSeeder(_dbConnection);
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await ResetDatabaseAsync();
    }

    public override Task InitializeAsync() => base.InitializeAsync();

    public sealed class InsertAsync : TournamentRepositoryTests
    {
        public InsertAsync(PostgresDatabaseFixture factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_AddTournamentToDb_When_TournamentIsInserted()
        {
            // Arrange
            User user = await _dataSeeder.SeedRandomUserAsync();
            Competition competition = await _dataSeeder.SeedRandomCompetitionAsync();
            var tournamentToInsert = new Tournament("Test Tournament", user.Id, competition.Id, description: "A Tournament description");

            // Act
            int tournamentId = await _sut.InsertAsync(tournamentToInsert);

            // Assert
            //var tournament = await _sut.FindByIdAsync(tournamentId);
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
        public RemoveAsync(PostgresDatabaseFixture factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_DeleteTournamentFromDb_When_TournamentExists()
        {
            // Arrange
            User user = await _dataSeeder.SeedRandomUserAsync();
            Competition competition = await _dataSeeder.SeedRandomCompetitionAsync();
            var tournamentToInsert = new Tournament("Test Tournament", user.Id, competition.Id, description: "A Tournament description");
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
        public async Task Should_ThrowEntityNotFoundException_When_TournamentDoesNotExist()
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
            User user = await _dataSeeder.SeedRandomUserAsync();
            var seedResult = await _dataSeeder.SeedFullCompetitionAsync();
            var tournamentToInsert = new Tournament("Test Tournament", user.Id, seedResult.Competition.Id, description: "A Tournament description");
            int tournamentId = await _sut.InsertAsync(tournamentToInsert);
            var teamRepository = new TeamRepository(_dbConnection);

            Faker faker = new Faker();
            for (int i = 0; i < 3; i++)
            {
                var availablePlayers = seedResult.Clubs.SelectMany(c => c.Players.OrderBy(_ => Guid.NewGuid()).Take(2)).ToList();
                var teamPlayers = faker.PickRandom(availablePlayers, 11).ToList();
                Team team = new TeamFaker(players: teamPlayers)
                    .RuleFor(t => t.UserId, user.Id)
                    .RuleFor(t => t.TournamentId, tournamentId)
                    .Generate();

                await teamRepository.InsertAsync(team);
            }

            // Act
            await _sut.RemoveAsync(tournamentId);

            // Assert
            string verifyTeamDeletionSql = @"
    SELECT id
    FROM team_tactics.user_team
    WHERE user_tournament_id = @TournamentId";
            var parameters = new DynamicParameters();
            parameters.Add("TournamentId", tournamentId);

            var teamIds = await _dbConnection.QueryAsync<int>(verifyTeamDeletionSql, parameters);

            Assert.Empty(teamIds);
        }
    }

    public sealed class FindIdByInviteCodeAsync : TournamentRepositoryTests
    {
        public FindIdByInviteCodeAsync(PostgresDatabaseFixture factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_ReturnTournamentId_When_InviteCodeExists()
        {
            // Arrange
            User user = await _dataSeeder.SeedRandomUserAsync();
            Competition competition = await _dataSeeder.SeedRandomCompetitionAsync();
            var tournamentToInsert = new Tournament("Test Tournament", user.Id, competition.Id, description: "A Tournament description");
            int tournamentId = await _sut.InsertAsync(tournamentToInsert);

            // Act
            int? foundTournamentId = await _sut.FindIdByInviteCodeAsync(tournamentToInsert.InviteCode);

            // Assert
            Assert.Equal(tournamentId, foundTournamentId);
        }

        [Fact]
        public async Task Should_ReturnNull_When_InviteCodeDoesNotExist()
        {
            // Arrange
            string inviteCode = "non-existent-code";

            // Act
            int? foundTournamentId = await _sut.FindIdByInviteCodeAsync(inviteCode);

            // Assert
            Assert.Null(foundTournamentId);
        }
    }

    public sealed class GetTournamentDetailsAsync : TournamentRepositoryTests
    {
        public GetTournamentDetailsAsync(PostgresDatabaseFixture factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_ReturnTournamentDetails_When_TournamentExists()
        {
            // Arrange
            User user = await _dataSeeder.SeedRandomUserAsync();
            Competition competition = await _dataSeeder.SeedRandomCompetitionAsync();
            var tournamentToInsert = new Tournament("Test Tournament", user.Id, competition.Id, description: "A Tournament description");
            int tournamentId = await _sut.InsertAsync(tournamentToInsert);
            // Act
            var tournamentDetails = await _sut.GetTournamentDetailsAsync(tournamentId);
            // Assert
            Assert.NotNull(tournamentDetails);
            Assert.Equal(tournamentToInsert.Name, tournamentDetails.Name);
            Assert.Equal(tournamentToInsert.Description, tournamentDetails.Description);
            Assert.Equal(tournamentToInsert.InviteCode, tournamentDetails.InviteCode);
            Assert.Equal(competition.Name, tournamentDetails.CompetitionName);
            Assert.Equal(user.Id, tournamentDetails.OwnerUserId);
            Assert.Equal(user.Username, tournamentDetails.OwnerUsername);
        }

        [Fact]
        public async Task Should_ThrowEntityNotFoundException_When_TournamentDoesNotExist()
        {
            // Arrange
            int tournamentId = 99;

            // Act
            var act = async () => await _sut.GetTournamentDetailsAsync(tournamentId);

            // Assert
            var ex = await Assert.ThrowsAsync<EntityNotFoundException>(act);
            Assert.Equal(nameof(Tournament), ex.EntityType);
            Assert.Equal(tournamentId, ex.Key);
        }
    }
}
