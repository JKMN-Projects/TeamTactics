namespace TeamTactics.Domain.Teams.Exceptions
{
    public sealed class NoCaptainException : DomainException
    {
        public NoCaptainException()
            : base("Team.NoCaptain", "The team does not have a captain.")
        {
        }
    }
}
