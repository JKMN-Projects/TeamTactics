using TeamTactics.Application.Common.Interfaces;

namespace TeamTactics.Application.Matches;

public interface IMatchRepository : IRepository<Match, int>
{
    Task<IEnumerable<MatchDto>> GetMatchesByTournamentIdAsync(int tournamentId);
}
