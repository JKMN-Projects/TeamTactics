namespace TeamTactics.Domain.Teams.Exceptions
{
    public sealed class TeamNotFullException : DomainException
    {
        public const int REQUIRED_NUMBER_OF_PLAYERS = 11;

        public TeamNotFullException(int playerCount)
            : base("Team.NotFull", $"The team is not full and can have more players. The number of players is {playerCount} out of the required {REQUIRED_NUMBER_OF_PLAYERS}")
        {
        }
    }
}
