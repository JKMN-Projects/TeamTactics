using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Bulletins;

namespace TeamTactics.Application.Bulletins
{
    public interface IBulletinRepository : ICrudRepository<Bulletin, int>
    {
        public Task<IEnumerable<BulletinDto>> FindInTournamentAsync(int tournamentId);
        public Task<bool> GetIfBulletinOwner(int userId, int bulletinId);
    }
}
