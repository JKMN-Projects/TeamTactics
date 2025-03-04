namespace TeamTactics.Domain.Teams.Exceptions
{
    public sealed class TeamFullException : DomainException
    {
        public TeamFullException()
            : base("Team.Full", "The team is full and cannot have more players.")
        {
        }
    }
}
