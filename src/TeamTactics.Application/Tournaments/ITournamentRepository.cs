using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Tournaments;

namespace TeamTactics.Application.Tournaments
{
    public interface ITournamentRepository : ICrudRepository<Tournament, int>
    {
        public Task<int?> FindIdByInviteCodeAsync(string inviteCode);

        Task<IEnumerable<UserTournamentTeamDto>> GetJoinedTournamentsAsync(int userId);
        Task<IEnumerable<TournamentTeamDto>> GetTeamsInTournamentAsync(int tournamentId);
        Task<IEnumerable<Tournament>> GetOwnedTournamentsAsync(int ownerId);
        Task UpdateOwnerAsync(int tournamentId, int previousOwnerId, int newOwnerId);
        Task<TournamentDetailsDto> GetTournamentDetailsAsync(int tournamentId);
    }
}