namespace TeamTactics.Domain.Teams.Exceptions
{
    public sealed class PlayerNotOnTeamException : DomainException
    {
        public PlayerNotOnTeamException(int playerId) 
            : base("Team.PlayerNotOnTeam", $"A player with id '{playerId}' is not in the team.")
        {
        }
    }
}
