using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Tournaments;
using TeamTactics.Infrastructure.Database.Repositories;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories
{
    public abstract class TeamRepositoryTests : TestBase
    {
        private readonly TeamRepository _sut;

        protected TeamRepositoryTests(CustomWebApplicationFactory factory) : base(factory)
        {
            _sut = new TeamRepository(_dbConnection);
        }

        private async Task<Team> SeedTeamAsync()
        {
            int userId = await _dbConnection.SeedUserAsync();
            int competitionId = await _dbConnection.SeedCompetitonAsync();
            var tournamentToInsert = new Tournament("Test Tournament", userId, competitionId, description: "A Tournament description");
            var tournamentRepository = GetService<ITournamentRepository>();
            int tournamentId = await tournamentRepository.InsertAsync(tournamentToInsert);

            Queue<Club> clubs = new Queue<Club>(new ClubFaker()
                .Generate(11));
            foreach (var club in clubs)
            {
                await _dbConnection.SeedClub(club);
            }

            List<Player> players = [];
            for (int i = 0; i < 11; i++)
            {
                var club = clubs.Dequeue();
                var player = new PlayerFaker(clubs: [club]).Generate();
                int playerId = await _dbConnection.SeedPlayer(player);
                players.Add(player);
            }
            Team team = new TeamFaker(players: players)
                .RuleFor(t => t.UserId, userId)
                .RuleFor(t => t.TournamentId, tournamentId)
                .Generate();
            int teamId = await _sut.InsertAsync(team);

            return team;
        }

        public sealed class FindByIdAsync : TeamRepositoryTests
        {
            public FindByIdAsync(CustomWebApplicationFactory factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_ReturnTeam_When_TeamExists()
            {
                // Arrange
                Team team = await SeedTeamAsync();

                // Act
                Team? result = await _sut.FindByIdAsync(team.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(team.Id, result.Id);
                Assert.Equal(team.Name, result.Name);
                Assert.Equal(team.Status, result.Status);
                Assert.Equal(team.UserId, result.UserId);
                Assert.Equal(team.TournamentId, result.TournamentId);
                Assert.Equal(team.Players.Count, result.Players.Count);
                Assert.All(team.Players, (player) =>
                {
                    var resultPlayer = result.Players.Single(p => p.PlayerId == player.PlayerId);
                    Assert.Equal(player.PlayerId, resultPlayer.PlayerId);
                    Assert.Equal(player.ClubId, resultPlayer.ClubId);
                    Assert.Equal(player.IsCaptain, resultPlayer.IsCaptain);
                });
            }

            [Fact]
            public async Task Should_ReturnNull_When_TeamDoesNotExist()
            {
                // Arrange
                int teamId = 99;

                // Act
                Team? result = await _sut.FindByIdAsync(teamId);

                // Assert
                Assert.Null(result);
            }
        }

        public sealed class InsertAsync : TeamRepositoryTests
        {
            public InsertAsync(CustomWebApplicationFactory factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_AddTeamToDb_When_TeamIsInserted()
            {
                // Arrange
                Team team = await SeedTeamAsync();

                // Act
                int teamId = await _sut.InsertAsync(team);

                // Assert
                var insertedTeam = await _sut.FindByIdAsync(teamId);

                Assert.NotNull(insertedTeam);
                Assert.Equal(teamId, insertedTeam.Id);
                Assert.Equal(team.Name, insertedTeam.Name);
                Assert.Equal(team.Status, insertedTeam.Status);
                Assert.Equal(team.UserId, insertedTeam.UserId);
                Assert.Equal(team.TournamentId, insertedTeam.TournamentId);
                Assert.Equal(team.Players.Count, insertedTeam.Players.Count);
                Assert.All(team.Players, (player) =>
                {
                    var resultPlayer = insertedTeam.Players.Single(p => p.PlayerId == player.PlayerId);
                    Assert.Equal(player.PlayerId, resultPlayer.PlayerId);
                    Assert.Equal(player.ClubId, resultPlayer.ClubId);
                    Assert.Equal(player.IsCaptain, resultPlayer.IsCaptain);
                });
            }
        }

        public sealed class UpdateAsync : TeamRepositoryTests
        {
            public UpdateAsync(CustomWebApplicationFactory factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_UpdateTeam_When_TeamExists()
            {
                // Arrange
                Team team = await SeedTeamAsync();

                Team updatedTeam = await _sut.FindByIdAsync(team.Id) ?? throw new Exception("Unable to get the team.");
                updatedTeam.Rename("Updated Team Name");

                var playerToRemove = updatedTeam.Players.First();
                updatedTeam.RemovePlayer(playerToRemove.PlayerId);

                Club clubToAdd = new ClubFaker().Generate();
                await _dbConnection.SeedClub(clubToAdd);

                Player playerToAdd = new PlayerFaker(clubs: [clubToAdd]).Generate();
                int playerToAddId = await _dbConnection.SeedPlayer(playerToAdd);
                updatedTeam.AddPlayer(playerToAdd);
                updatedTeam.SetCaptain(playerToAddId);

                // Act
                await _sut.UpdateAsync(updatedTeam);

                // Assert
                var result = await _sut.FindByIdAsync(team.Id);
                Assert.NotNull(result);
                Assert.Equal(team.Id, result.Id);
                Assert.Equal(updatedTeam.Name, result.Name);
                Assert.Equal(updatedTeam.Players.Count, result.Players.Count);
                Assert.DoesNotContain(result.Players, p => p.PlayerId == playerToRemove.PlayerId);
                Assert.Contains(result.Players, p => p.PlayerId == playerToAddId && p.IsCaptain);
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TeamDoesNotExists()
            {
                // Arrange
                Team team = new TeamFaker()
                    .RuleFor(t => t.Id, 99)
                    .Generate();

                // Act
                Func<Task> act = async () => await _sut.UpdateAsync(team);

                // Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(act);
            }
        }

        public sealed class RemoveAsync : TeamRepositoryTests
        {
            public RemoveAsync(CustomWebApplicationFactory factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_DeleteTeamAndPlayers_When_TeamExists()
            {
                // Arrange
                Team team = await SeedTeamAsync();

                // Act
                await _sut.RemoveAsync(team.Id);

                // Assert
                var result = await _sut.FindByIdAsync(team.Id);
                Assert.Null(result);
                var parameters = new DynamicParameters();
                parameters.Add("TeamId", team.Id);
                string verifyPlayerDeletionSql = @"
                    SELECT player_id
                    FROM team_tactics.player_user_team
                    WHERE user_team_id = @TeamId";
                var playerIds = await _dbConnection.QueryAsync<int>(verifyPlayerDeletionSql, parameters);
                Assert.Empty(playerIds);
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TeamDoesNotExists()
            {
                // Arrange
                int teamId = 99;

                // Act
                Func<Task> act = async () => await _sut.RemoveAsync(teamId);

                // Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(act);
            }
        }
    }
}
