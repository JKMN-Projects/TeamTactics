
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Teams.Exceptions;
using TeamTactics.Fixtures;

namespace TeamTactics.Domain.UnitTests.Teams
{
    public abstract class TeamTests : TestBase
    {
        public sealed class AddPlayer : TeamTests
        {
            [Fact]
            public void Should_AddPlayer_When_TeamIsEmpty()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 0).Generate();
                Player player = new PlayerFaker().Generate();

                // Act
                team.AddPlayer(player);

                // Assert
                Assert.Contains(player.Id, team.Players.Select(p => p.PlayerId));
            }

            [Fact]
            public void Should_AddNewPlayer_When_TeamIsNotEmpty()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 5).Generate();
                Player player = new PlayerFaker()
                    .RuleFor(x => x.Id, 6)
                    .Generate();

                // Act
                team.AddPlayer(player);

                // Assert
                Assert.Contains(player.Id, team.Players.Select(p => p.PlayerId));
            }

            [Fact]
            public void ShouldThrow_PlayerAlreadyInTeamExceptionn_When_PlayerIsAlreadyOnTeam()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 5).Generate();
                Player player = new PlayerFaker()
                    .RuleFor(x => x.Id, 6)
                    .Generate();
                team.AddPlayer(player);

                // Act
                Action act = () => team.AddPlayer(player);

                // Assert
                Assert.Throws<PlayerAlreadyInTeamException>(act);
            }

            [Fact]
            public void ShouldThrow_TeamIsFullException_When_TeamIsFull()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 11).Generate();
                Player player = new PlayerFaker()
                    .RuleFor(x => x.Id, 12)
                    .Generate();

                // Act
                Action act = () => team.AddPlayer(player);

                // Assert
                Assert.ThrowsAny<TeamFullException>(act);
            }

            [Fact]
            public void ShouldThrow_TeamLockedException_When_TeamIsLocked()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 5).Generate();
                team.Lock();

                Player player = new PlayerFaker()
                    .RuleFor(x => x.Id, 6)
                    .Generate();

                // Act
                Action act = () => team.AddPlayer(player);

                // Assert
                Assert.Throws<TeamLockedException>(act);
            }

            [Fact]
            public void ShouldThrow_MaximumPlayersFromClubReachedException_When_PlayerPerClubLimitIsReached()
            {
                // Arrange
                const int CLUB_ID = 29;
                Team team = new TeamFaker(playerCount: 0).Generate();
                Player player = new PlayerFaker()
                    .RuleFor(x => x.Id, 10)
                    .Generate();
                Player preExistingPlayer = new PlayerFaker()
                    .RuleFor(x => x.Id, 6)
                    .Generate();
                Player preExistingPlayer2 = new PlayerFaker()
                    .RuleFor(x => x.Id, 7)
                    .Generate();
                player.SignContract(CLUB_ID);
                preExistingPlayer.SignContract(CLUB_ID);
                preExistingPlayer2.SignContract(CLUB_ID);
                team.AddPlayer(preExistingPlayer);
                team.AddPlayer(preExistingPlayer2);

                // Act
                Action act = () => team.AddPlayer(player);

                // Assert
                Assert.Throws<MaximumPlayersFromSameClubReachedException>(act);
            }
        }

        public sealed class RemovePlayer : TeamTests
        {
            [Fact]
            public void Should_RemovePlayer_When_PlayerIsOnTeam()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 5).Generate();
                int playerId = team.Players.First().PlayerId;

                // Act
                team.RemovePlayer(playerId);

                // Assert
                Assert.DoesNotContain(playerId, team.Players.Select(p => p.PlayerId));
            }

            [Fact]
            public void ShouldThrow_PlayerNotOnTeamException_When_PlayerIsNotOnTeam()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 5).Generate();
                int playerId = 6;

                // Act
                Action act = () => team.RemovePlayer(playerId);

                // Assert
                Assert.Throws<PlayerNotOnTeamException>(act);
            }

            [Fact]
            public void ShouldThrow_TeamLockedExceotion_When_TeamIsLocked()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 5).Generate();
                team.Lock();
                int playerId = team.Players.First().PlayerId;

                // Act
                Action act = () => team.RemovePlayer(playerId);

                // Assert
                Assert.Throws<TeamLockedException>(act);
            }
        }

        public sealed class SetCaptain : TeamTests
        {
            [Fact]
            public void Should_SetCaptain_When_PlayerIsOnTeam()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 5).Generate();
                int playerId = team.Players.Last().PlayerId;

                // Act
                team.SetCaptain(playerId);

                // Assert
                Assert.True(team.Players.First(p => p.PlayerId == playerId).IsCaptain);
            }

            [Fact]
            public void ShouldThrow_PlayerNotOnTeamException_When_PlayerIsNotOnTeam()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 5).Generate();
                int playerId = 6;

                // Act
                Action act = () => team.SetCaptain(playerId);

                // Assert
                Assert.Throws<PlayerNotOnTeamException>(act);
            }

            [Fact]
            public void ShouldThrow_TeamLockedException_When_TeamIsLocked()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 5).Generate();
                team.Lock();
                int playerId = team.Players.First().PlayerId;

                // Act
                Action act = () => team.SetCaptain(playerId);

                // Assert
                Assert.Throws<TeamLockedException>(act);
            }
        }

        public sealed class Lock : TeamTests
        {
            [Fact]
            public void Should_LockTeam_When_TeamIsCompleted()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 11).Generate();

                // Act
                team.Lock();

                // Assert
                Assert.Equal(TeamStatus.Locked, team.Status);
            }

            [Fact]
            public void ShouldThrow_TeamLockedException_When_TeamIsAlreadyLocked()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 11).Generate();
                team.Lock();

                // Act
                Action act = () => team.Lock();

                // Assert
                Assert.Throws<TeamLockedException>(act);
            }

            [Fact]
            public void ShouldThrow_TeamNotFullException_When_TeamIsNotFull()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 10).Generate();

                // Act
                Action act = () => team.Lock();

                // Assert
                Assert.Throws<TeamNotFullException>(act);
            }

            [Fact]
            public void ShouldThrow_NoCaptainException_When_TeamCaptainIsNotRequired()
            {
                // Arrange
                Team team = new TeamFaker(playerCount: 11).Generate();
                var currentCaptain = team.Players.Single(p => p.IsCaptain);
                team.RemovePlayer(currentCaptain.PlayerId);
                var replacementPlayer = new PlayerFaker()
                    .RuleFor(x => x.Id, 12)
                    .Generate();
                team.AddPlayer(replacementPlayer);

                // Act
                Action act = () => team.Lock();

                // Assert
                Assert.Throws<NoCaptainException>(act);
            }
        }
    }
}
