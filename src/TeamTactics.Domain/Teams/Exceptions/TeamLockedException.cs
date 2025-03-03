namespace TeamTactics.Domain.Teams.Exceptions
{
    public sealed class TeamLockedException : DomainException
    {
        public TeamLockedException()
            : base("Team.Locked", "The team is locked and cannot be modified.")
        {
        }
    }
}
