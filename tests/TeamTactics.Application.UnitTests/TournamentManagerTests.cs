using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Competitions;
using TeamTactics.Application.Matches;
using TeamTactics.Application.Points;
using TeamTactics.Application.Teams;
using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Tournaments;
using TeamTactics.Domain.Tournaments.Exceptions;
using TeamTactics.Fixtures;

namespace TeamTactics.Application.UnitTests
{
    public abstract class TournamentManagerTests : TestBase
    {
        private readonly TournamentManager _sut;
        private readonly ITournamentRepository _tournamentRepositoryMock;
        private readonly ICompetitionRepository _competitionRepositoryMock;
        private readonly ITeamRepository _teamRepositoryMock;
        private readonly IPointsRepository _pointsRepositoryMock;
        private readonly IMatchRepository _matchRepositoryMock;

        protected TournamentManagerTests()
        {
            _tournamentRepositoryMock = Substitute.For<ITournamentRepository>();
            _competitionRepositoryMock = Substitute.For<ICompetitionRepository>();
            _teamRepositoryMock = Substitute.For<ITeamRepository>();
            _pointsRepositoryMock = Substitute.For<IPointsRepository>();
            _matchRepositoryMock = Substitute.For<IMatchRepository>();
            _sut = new TournamentManager(
                _tournamentRepositoryMock,
                _competitionRepositoryMock,
                _teamRepositoryMock,
                _pointsRepositoryMock,
                _matchRepositoryMock);
        }

        public sealed class CreateTournamentAsync : TournamentManagerTests
        {
            [Fact]
            public async Task Should_AddTournamentToRepository_When_CompetitionExists()
            {
                // Arrange
                var name = "TournamentName";
                var teamName = "TeamName";
                var competitionId = 1;
                var createdByUserId = 5;
                var competition = new CompetitionFaker().Generate();

                _competitionRepositoryMock.FindByIdAsync(competitionId).Returns(competition);

                // Act
                var result = await _sut.CreateTournamentAsync(name, teamName, competitionId, createdByUserId);

                // Assert
                await _tournamentRepositoryMock.Received(1).InsertAsync(Arg.Is<Tournament>(t => t.Name == name && t.CreatedByUserId == createdByUserId && t.CompetitionId == competitionId));
                await _teamRepositoryMock.Received(1).InsertAsync(Arg.Is<Team>(t => t.Name == teamName && t.UserId == createdByUserId));
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_CompetitionDoesNotExist()
            {
                // Arrange
                var name = "TournamentName";
                var teamName = "TeamName";
                var competitionId = 5;
                var createdByUserId = 1;

                _competitionRepositoryMock.FindByIdAsync(competitionId).Returns((Competition)null!);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.CreateTournamentAsync(name, teamName, competitionId, createdByUserId));
            }
        }

        public sealed class DeleteTournamentAsync : TournamentManagerTests
        {
            [Fact]
            public async Task Should_RemoveTournamentFromRepository_When_UserIsAuthorized()
            {
                // Arrange
                var tournamentId = 5;
                var userId = 1;
                int competitionId = 10;
                var tournament = new Tournament("TournamentName", userId, competitionId);

                _tournamentRepositoryMock.FindByIdAsync(tournamentId).Returns(tournament);

                // Act
                await _sut.DeleteTournamentAsync(tournamentId, userId);

                // Assert
                await _tournamentRepositoryMock.Received(1).RemoveAsync(tournamentId);
            }

            [Fact]
            public async Task Should_ThrowUnauthorizedAccessException_When_UserIsNotAuthorized()
            {
                // Arrange
                int tournamentId = 100;
                int userId = 1;
                int competitionId = 10;
                int otherUserId = 3;
                var tournament = new Tournament("TournamentName", otherUserId, competitionId);

                _tournamentRepositoryMock.FindByIdAsync(tournamentId).Returns(tournament);

                // Act & Assert
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteTournamentAsync(tournamentId, userId));
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TournamentDoesNotExist()
            {
                // Arrange
                var tournamentId = 3;
                var userId = 1;

                _tournamentRepositoryMock.FindByIdAsync(tournamentId).Returns((Tournament)null!);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.DeleteTournamentAsync(tournamentId, userId));
            }
        }

        public sealed class UpdateTournamentAsync : TournamentManagerTests
        {
            [Fact]
            public async Task Should_UpdateTournamentInRepository_When_UserIsAuthorized()
            {
                // Arrange
                var userId = 1;
                var tournamentId = 3;
                int competitionId = 10;
                var name = "UpdatedName";
                var description = "UpdatedDescription";
                var tournament = new Tournament("TournamentName", userId, competitionId);

                _tournamentRepositoryMock.FindByIdAsync(tournamentId).Returns(tournament);

                // Act
                await _sut.UpdateTournamentAsync(userId, tournamentId, name, description);

                // Assert
                await _tournamentRepositoryMock.Received(1).UpdateAsync(Arg.Is<Tournament>(t => t.Name == name && t.Description == description));
            }

            [Fact]
            public async Task Should_ThrowUnauthorizedException_When_UserIsNotAuthorized()
            {
                // Arrange
                var userId = 1;
                var otherUserId = 5;
                var tournamentId = 3;
                int competitionId = 10;
                var name = "UpdatedName";
                var description = "UpdatedDescription";
                var tournament = new Tournament("TournamentName", otherUserId, competitionId);

                _tournamentRepositoryMock.FindByIdAsync(tournamentId).Returns(tournament);

                // Act & Assert
                await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.UpdateTournamentAsync(userId, tournamentId, name, description));
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TournamentDoesNotExist()
            {
                // Arrange
                var userId = 1;
                var tournamentId = 3;
                var name = "UpdatedName";
                var description = "UpdatedDescription";

                _tournamentRepositoryMock.FindByIdAsync(tournamentId).Returns((Tournament)null!);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.UpdateTournamentAsync(userId, tournamentId, name, description));
            }
        }

        public sealed class JoinTournamentAsync : TournamentManagerTests
        {
            [Fact]
            public async Task Should_AddTeamToRepository_When_TournamentExists()
            {
                // Arrange
                var userId = 1;
                var inviteCode = "InviteCode";
                var teamName = "TeamName";
                var tournamentId = 1;

                _tournamentRepositoryMock.FindIdByInviteCodeAsync(inviteCode).Returns(tournamentId);

                // Act
                var result = await _sut.JoinTournamentAsync(userId, inviteCode, teamName);

                // Assert
                await _teamRepositoryMock.Received(1).InsertAsync(Arg.Is<Team>(t => t.Name == teamName && t.UserId == userId && t.TournamentId == tournamentId));
            }

            [Fact]
            public async Task Should_ThrowEntityNotFoundException_When_TournamentDoesNotExist()
            {
                // Arrange
                var userId = 1;
                var inviteCode = "InviteCode";
                var teamName = "TeamName";

                _tournamentRepositoryMock.FindIdByInviteCodeAsync(inviteCode).Returns((int?)null);

                // Act & Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.JoinTournamentAsync(userId, inviteCode, teamName));
            }


            [Fact]
            public async Task Should_AlreadyJoinedTournamentException_When_AlreadyJoinedTouranment()
            {
                // Arrange
                var userId = 1;
                var inviteCode = "InviteCode";
                var teamName = "TeamName";
                var tournamentId = 1;

                _tournamentRepositoryMock.FindIdByInviteCodeAsync(inviteCode).Returns(tournamentId);
                await _sut.JoinTournamentAsync(userId, inviteCode, teamName);
                _tournamentRepositoryMock.IsUserTournamentMember(userId, tournamentId).Returns(true);

                // Act & Assert
                await Assert.ThrowsAsync<AlreadyJoinedTournamentException>(() => _sut.JoinTournamentAsync(userId, inviteCode, teamName));
            }
        }
    }
}
