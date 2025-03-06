using System.Security.Cryptography;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Tournaments;

namespace TeamTactics.Application.Tournaments
{
    public interface ITournamentRepository : ICrudRepository<Tournament, int>
    {
        public Task<bool> CheckTournamentInviteCodeAsync(string inviteCode);
    }
}