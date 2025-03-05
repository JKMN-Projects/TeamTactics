
using TeamTactics.Domain.Players;

namespace TeamTactics.Domain.Teams.Exceptions
{
    public sealed class PlayerAlreadyInTeamException : DomainException
    {
        public PlayerAlreadyInTeamException(Player player)
            : base("Team.PlayerAlreadyInTeam", $"The player '{player.FirstName} {player.LastName}' is already in the team.")
        {
        }
    }
}
