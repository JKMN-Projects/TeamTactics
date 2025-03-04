namespace TeamTactics.Domain.Teams.Exceptions
{
    public sealed class MaximumPlayersFromSameClubReachedException : DomainException
    {
        public MaximumPlayersFromSameClubReachedException(int clubId)
            : base("Team.MaxPlayersFromSameClub", $"The team has reached the maximum number of players from the same club with id '{clubId}'.")
        {
        }
    }
}
