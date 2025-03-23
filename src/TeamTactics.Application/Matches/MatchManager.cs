
namespace TeamTactics.Application.Matches
{
    public sealed class MatchManager
    {
        private readonly IMatchRepository _matchRepository;

        public MatchManager(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }


        public async Task<IEnumerable<MatchDto>> GetTournamentMatches(int tournamentId)
        {
            return await _matchRepository.GetMatchesByTournamentIdAsync(tournamentId);
        }
    }
}
