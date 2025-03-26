using Microsoft.Extensions.Logging;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Players;
using TeamTactics.Application.Points;
using TeamTactics.Application.Teams;
using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Tournaments.Exceptions;
using TeamTactics.Fixtures;

namespace TeamTactics.Application.UnitTests
{
    public abstract class TeamManagerTests : TestBase
    {
        private readonly TeamManager _sut;
        private readonly ITeamRepository _teamRepositoryMock;
        private readonly IPointsRepository _pointsRepositoryMock;
        private readonly IPlayerRepository _playerRepositoryMock;
        private readonly ITournamentRepository _tournamentRepositoryMock;
        private readonly ILogger<TeamManager> _loggerMock;

        protected TeamManagerTests()
        {
            _teamRepositoryMock = Substitute.For<ITeamRepository>();
            _pointsRepositoryMock = Substitute.For<IPointsRepository>();
            _playerRepositoryMock = Substitute.For<IPlayerRepository>();
            _tournamentRepositoryMock = Substitute.For<ITournamentRepository>();
            _loggerMock = Substitute.For<ILogger<TeamManager>>();

            _sut = new TeamManager(
                _teamRepositoryMock,
                _pointsRepositoryMock,
                _playerRepositoryMock,
                _tournamentRepositoryMock,
                _loggerMock);
        }

        public sealed class AddPlayerToTeamAsync : TeamManagerTests
        {
            [Fact]
            public async Task Should_AddPlayerToTeam_When_PlayerAndTeamExist()
            {
                // Arrange
                const int teamId = 1;
                Team team = new TeamFaker(playerCount: 5).Generate();
                int playerId = team.Players.OrderByDescending(p => p.PlayerId).First().PlayerId + 1;
                Player player = new PlayerFaker()
                    .RuleFor(p => p.Id, playerId)
                    .Generate();

                _playerRepositoryMock.FindByIdAsync(playerId).Returns(player);
                _teamRepositoryMock.FindByIdAsync(teamId).Returns(team);

                // Act
                await _sut.AddPlayerToTeamAsync(teamId, playerId);

                // Assert
                await _teamRepositoryMock.Received(1).UpdateAsync(team);
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_PlayerDoesNotExist()
            {
                // Arrange
                const int teamId = 1;
                const int playerId = 2;

                _playerRepositoryMock.FindByIdAsync(playerId).Returns((Player)null!);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.AddPlayerToTeamAsync(teamId, playerId));
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TeamDoesNotExist()
            {
                // Arrange
                const int teamId = 1;
                const int playerId = 2;
                Player player = new PlayerFaker().Generate();

                _playerRepositoryMock.FindByIdAsync(playerId).Returns(player);
                _teamRepositoryMock.FindByIdAsync(teamId).Returns((Team)null!);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.AddPlayerToTeamAsync(teamId, playerId));
            }
        }

        public sealed class DeleteTeamAsync : TeamManagerTests
        {
            [Fact]
            public async Task Should_RemoveTeamFromRepository_When_TeamExists()
            {
                // Arrange
                const int teamId = 1;
                Team team = new TeamFaker().Generate();

                _teamRepositoryMock.FindByIdAsync(teamId).Returns(team);

                // Act
                await _sut.DeleteTeamAsync(teamId);

                // Assert
                await _teamRepositoryMock.Received(1).RemoveAsync(teamId);
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TeamDoesNotExist()
            {
                // Arrange
                const int teamId = 1;

                _teamRepositoryMock.FindByIdAsync(teamId).Returns((Team)null!);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.DeleteTeamAsync(teamId));
            }
        }

        public sealed class LockTeamAsync : TeamManagerTests
        {
            [Fact]
            public async Task Should_LockTeam_When_TeamExists()
            {
                // Arrange
                const int teamId = 1;
                Team team = new TeamFaker().Generate();

                _teamRepositoryMock.FindByIdAsync(teamId).Returns(team);

                // Act
                await _sut.LockTeamAsync(teamId);

                // Assert
                await _teamRepositoryMock.Received(1).UpdateAsync(team);
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TeamDoesNotExist()
            {
                // Arrange
                const int teamId = 1;

                _teamRepositoryMock.FindByIdAsync(teamId).Returns((Team)null!);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.LockTeamAsync(teamId));
            }
        }

        public sealed class RemovePlayerFromTeamAsync : TeamManagerTests
        {
            [Fact]
            public async Task Should_RemovePlayerFromTeam_When_TeamExists()
            {
                // Arrange
                const int teamId = 1;
                const int playerId = 2;
                Team team = new TeamFaker().Generate();

                _teamRepositoryMock.FindByIdAsync(teamId).Returns(team);

                // Act
                await _sut.RemovePlayerFromTeamAsync(teamId, playerId);

                // Assert
                await _teamRepositoryMock.Received(1).UpdateAsync(team);
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TeamDoesNotExist()
            {
                // Arrange
                const int teamId = 1;
                const int playerId = 2;

                _teamRepositoryMock.FindByIdAsync(teamId).Returns((Team)null!);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.RemovePlayerFromTeamAsync(teamId, playerId));
            }
        }

        public sealed class SetTeamCaptainAsync : TeamManagerTests
        {
            [Fact]
            public async Task Should_SetTeamCaptain_When_TeamExists()
            {
                // Arrange
                Team team = new TeamFaker().Generate();
                const int teamId = 1;
                int playerId = team.Players.Where(p => !p.IsCaptain).First().PlayerId;

                _teamRepositoryMock.FindByIdAsync(teamId).Returns(team);

                // Act
                await _sut.SetTeamCaptainAsync(teamId, playerId);

                // Assert
                await _teamRepositoryMock.Received(1).UpdateAsync(team);
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TeamDoesNotExist()
            {
                // Arrange
                const int teamId = 1;
                const int playerId = 2;

                _teamRepositoryMock.FindByIdAsync(teamId).Returns((Team)null!);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.SetTeamCaptainAsync(teamId, playerId));
            }
        }

        public sealed class RenameTeamAsync : TeamManagerTests
        {
            [Fact]
            public async Task Should_RenameTeam_When_UserIsAuthorized()
            {
                // Arrange
                const int userId = 1;
                const int teamId = 2;
                const string name = "NewTeamName";
                Team team = new TeamFaker()
                    .RuleFor(t => t.UserId, userId)
                    .Generate();

                _teamRepositoryMock.FindByIdAsync(teamId).Returns(team);

                // Act
                await _sut.RenameTeamAsync(userId, teamId, name);

                // Assert
                await _teamRepositoryMock.Received(1).UpdateAsync(team);
            }

            [Fact]
            public async Task Should_ThrowUnauthorizedException_When_UserIsNotAuthorized()
            {
                // Arrange
                const int userId = 1;
                const int OtherUserId = 2;
                const int teamId = 3;
                const string name = "NewTeamName";
                Team team = new TeamFaker()
                    .RuleFor(t => t.UserId, OtherUserId)
                    .Generate();

                _teamRepositoryMock.FindByIdAsync(teamId).Returns(team);

                // Act & Assert
                await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.RenameTeamAsync(userId, teamId, name));
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TeamDoesNotExist()
            {
                // Arrange
                const int userId = 1;
                const int teamId = 2;
                const string name = "NewTeamName";

                _teamRepositoryMock.FindByIdAsync(teamId).Returns((Team)null!);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.RenameTeamAsync(userId, teamId, name));
            }
        }
    }
}
