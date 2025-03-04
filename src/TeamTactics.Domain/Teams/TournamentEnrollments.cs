
namespace TeamTactics.Domain.Teams
{
    public class TournamentEnrollments
    {
        public int Id { get; private set; }
        public int TournamentId { get; private set; }
        public EnrollStatus Status { get; private set; }

        public TournamentEnrollments(int tournamentId)
        {
            TournamentId = tournamentId;
            Status = EnrollStatus.Pending;
        }
    }

    public enum EnrollStatus
    {
        Pending,
        Accepted,
        Rejected
    }
}
