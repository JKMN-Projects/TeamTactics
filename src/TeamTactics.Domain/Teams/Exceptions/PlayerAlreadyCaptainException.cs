namespace TeamTactics.Domain.Teams.Exceptions
{
    public sealed class PlayerAlreadyCaptainException : DomainException
    {
        public PlayerAlreadyCaptainException(int playerId)
            : base("Player.AlreadyCaptain", $"The player with id '{playerId}' is already the captain of the team.")
        {
        }
    }
}
