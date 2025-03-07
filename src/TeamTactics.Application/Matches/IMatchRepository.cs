using TeamTactics.Application.Common.Interfaces;

namespace TeamTactics.Application.Matches;

public interface IMatchRepository : IRepository<Match, int>
{
    Task<Match?> GetMatchesByTournamentIdAsync(int tournamentId);
}
