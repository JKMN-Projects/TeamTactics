using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Tournaments;

namespace TeamTactics.Application.Tournaments
{
    public interface ITournamentRepository : ICrudRepository<Tournament, int>
    {
        public Task<int?> FindByInviteCodeAsync(string inviteCode);
        Task<IEnumerable<Tournament>> GetJoinedTournamentsAsync(string userId);
        Task<IEnumerable<TournamentTeamsDto>> GetTeamsInTournamentAsync(int tournamentId);
        Task<IEnumerable<Tournament>> GetOwnedTournamentsAsync(int ownerId);
    }
}