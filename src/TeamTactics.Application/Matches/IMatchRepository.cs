using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Matches;

namespace TeamTactics.Application.Matches;

public interface IMatchRepository : IRepository<Match, int>
{
    Task<IEnumerable<MatchDto>> GetMatchesByTournamentIdAsync(int tournamentId);
}
