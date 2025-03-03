
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Teams.Exceptions;

namespace TeamTactics.Domain.UnitTests.Teams
{
    public abstract class TeamTests : TestBase
    {
        public sealed class AddPlayer : TeamTests
        {
            /*
            [Fact]
            public void Should_AddPlayerToTeam()
            {
                // Arrange
                var team = new Team();
                var player = new Player();
                // Act
                team.AddPlayer(player);
                // Assert
                Assert.Contains(player, team.Players);
            }
            [Fact]
            public void Should_ThrowPlayerNotOnTeamException_When_PlayerIsAlreadyOnTeam()
            {
                // Arrange
                var team = new Team();
                var player = new Player();
                team.AddPlayer(player);
                // Act
                Action act = () => team.AddPlayer(player);
                // Assert
                Assert.Throws<PlayerAlreadyInTeamException>(act);
            }
            */
        }
    }
}
