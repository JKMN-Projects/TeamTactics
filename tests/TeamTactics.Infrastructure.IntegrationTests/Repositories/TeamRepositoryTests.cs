using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Teams;
using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Tournaments;
using TeamTactics.Domain.Users;
using TeamTactics.Infrastructure.Database.Repositories;
using TeamTactics.Infrastructure.IntegrationTests.Seeding;

namespace TeamTactics.Infrastructure.IntegrationTests.Repositories
{
    public abstract class TeamRepositoryTests : RepositoryTestBase, IAsyncLifetime
    {
        private readonly TeamRepository _sut;
        private readonly DataSeeder _dataSeeder;

        protected TeamRepositoryTests(PostgresDatabaseFixture factory) : base(factory)
        {
            _sut = new TeamRepository(DbConnection);
            _dataSeeder = new DataSeeder(DbConnection);
        }

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync();
            await ResetDatabaseAsync();
        }

        public override Task InitializeAsync() => base.InitializeAsync();

        private async Task<Team> SeedTeamAsync(int playerCount = 11)
        {
            User user = await _dataSeeder.SeedRandomUserAsync();
            var seedResult = await _dataSeeder.SeedFullCompetitionAsync();

            var tournamentToInsert = new Tournament("Test Tournament", user.Id, seedResult.Competition.Id, description: "A Tournament description");
            var tournamentRepository = new TournamentRepository(DbConnection);
            int tournamentId = await tournamentRepository.InsertAsync(tournamentToInsert);

            List<Player> teamPlayers = [];
            if (playerCount > 0)
            {
                Faker faker = new Faker();
                var availablePlayers = seedResult.Clubs.SelectMany(c => c.Players.Take(2)).ToList();
                teamPlayers = faker.PickRandom(availablePlayers, playerCount).ToList();
            }

            Team team = new TeamFaker(players: teamPlayers)
                .RuleFor(t => t.UserId, user.Id)
                .RuleFor(t => t.TournamentId, tournamentId)
                .Generate();
            int teamId = await _sut.InsertAsync(team);

            return team;
        }

        public sealed class GetTeamPlayersByTeamIdAsync : TeamRepositoryTests
        {
            public GetTeamPlayersByTeamIdAsync(PostgresDatabaseFixture factory) : base(factory)
            {
            }
            [Fact]
            public async Task Should_ReturnTeamPlayers_When_TeamPlayersExists()
            {
                //Arrange
                Team team = await SeedTeamAsync();

                //act
                IEnumerable<TeamPlayerDto> players = await _sut.GetTeamPlayersByTeamIdAsync(team.Id);

                //assert

                Assert.NotEmpty(players);
            }
        }

        public sealed class FindByIdAsync : TeamRepositoryTests
        {
            public FindByIdAsync(PostgresDatabaseFixture factory) : base(factory)
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
            public async Task Should_ReturnTeam_When_TeamHasNoPlayers()
            {
                // Arrange
                Team team = await SeedTeamAsync(playerCount: 0);

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
                Assert.Empty(team.Players);
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
            public InsertAsync(PostgresDatabaseFixture factory) : base(factory)
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
            public UpdateAsync(PostgresDatabaseFixture factory) : base(factory)
            {
            }

            [Fact]
            public async Task Should_UpdateTeam_When_TeamExists()
            {
                // Arrange
                Team team = await SeedTeamAsync();

                Team updatedTeam = await _sut.FindByIdAsync(team.Id)
                    ?? throw new Exception("Unable to get the seeded team. Should never happen.");
                updatedTeam.Rename("Updated Team Name");

                var playerToRemove = updatedTeam.Players.First();
                updatedTeam.RemovePlayer(playerToRemove.PlayerId);

                Club clubToAdd = await _dataSeeder.SeedRandomClubAsync();
                Player playerToAdd = new PlayerFaker(clubs: [clubToAdd]).Generate();
                await _dataSeeder.SeedPlayerAsync(playerToAdd);

                updatedTeam.AddPlayer(playerToAdd);
                updatedTeam.SetCaptain(playerToAdd.Id);

                // Act
                await _sut.UpdateAsync(updatedTeam);

                // Assert
                var result = await _sut.FindByIdAsync(updatedTeam.Id);
                Assert.NotNull(result);
                Assert.Equal(updatedTeam.Id, result.Id);
                Assert.Equal(updatedTeam.Name, result.Name);
                Assert.Equal(updatedTeam.Players.Count, result.Players.Count);
                Assert.DoesNotContain(result.Players, p => p.PlayerId == playerToRemove.PlayerId);
                Assert.Contains(result.Players, p => p.PlayerId == playerToAdd.Id && p.IsCaptain);
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
            public RemoveAsync(PostgresDatabaseFixture factory) : base(factory)
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
                var playerIds = await DbConnection.QueryAsync<int>(verifyPlayerDeletionSql, parameters);
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

        public sealed class GetTeamDtoByIdAsync : TeamRepositoryTests
        {
            public GetTeamDtoByIdAsync(PostgresDatabaseFixture factory) : base(factory)
            {

            }

            [Fact]
            public async Task Should_ReturnTeam_When_TeamExists()
            {
                // Arrange
                Team team = await SeedTeamAsync();

                // Act
                TeamDto? result = await _sut.GetTeamDtoByIdAsync(team.Id);

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
                    var resultPlayer = result.Players.Single(p => p.Id == player.PlayerId);
                    Assert.Equal(player.PlayerId, resultPlayer.Id);
                    Assert.Equal(player.ClubId, resultPlayer.ClubId);
                    Assert.Equal(player.IsCaptain, resultPlayer.Captain);
                });
            }


            [Fact]
            public async Task Should_ReturnTeam_When_TeamHasNoPlayers()
            {
                // Arrange
                Team team = await SeedTeamAsync(playerCount: 0);

                // Act
                TeamDto? result = await _sut.GetTeamDtoByIdAsync(team.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(team.Id, result.Id);
                Assert.Equal(team.Name, result.Name);
                Assert.Equal(team.Status, result.Status);
                Assert.Equal(team.UserId, result.UserId);
                Assert.Equal(team.TournamentId, result.TournamentId);
                Assert.Equal(team.Players.Count, result.Players.Count);
                Assert.Empty(team.Players);
            }

            [Fact]
            public async Task Should_ReturnNull_When_TeamDoesNotExist()
            {
                // Arrange
                int teamId = 99;

                // Act
                TeamDto? result = await _sut.GetTeamDtoByIdAsync(teamId);

                // Assert
                Assert.Null(result);
            }
        }

    }
}
