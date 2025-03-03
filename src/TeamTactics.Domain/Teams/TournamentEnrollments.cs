
namespace TeamTactics.Domain.Teams
{
    public class TournamentEnrollments
    {
        public int Id { get; private set; }
        public int TournamentId { get; private set; }
        public bool Accept { get; private set; }

        public TournamentEnrollments(int tournamentId)
        {
            TournamentId = tournamentId;
        }
    }
}
