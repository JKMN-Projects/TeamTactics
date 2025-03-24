
using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Bulletins;
using TeamTactics.Domain.Tournaments;

namespace TeamTactics.Application.Bulletins
{
    public sealed class BulletinManager
    {
        private readonly IBulletinRepository _bulletinRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly TimeProvider _timeProvider;

        public BulletinManager(IBulletinRepository bulletinRepository, ITournamentRepository tournamentRepository, TimeProvider timeProvider)
        {
            _bulletinRepository = bulletinRepository;
            _tournamentRepository = tournamentRepository;
            _timeProvider = timeProvider;
        }

        public async Task<int> CreateBulletinAsync(string text, int tournamentId, int userId)
        {
            Tournament? tournament = await _tournamentRepository.FindByIdAsync(tournamentId);
            if (tournament == null)
            {
                throw EntityNotFoundException.ForEntity<Tournament>("Tournament does not exist");
            }

            bool isCreatorOrAttendee = tournament.CreatedByUserId == userId
                || (await _tournamentRepository.GetJoinedTournamentsAsync(userId)).Any(t => t.TournamentId == tournamentId);

            if (!isCreatorOrAttendee)
            {
                throw new UnauthorizedException("User is not authorized to create a bulletin in this tournament");
            }

            var bulletin = new Bulletin(
                text: text,
                createdTime: _timeProvider.GetUtcNow().UtcDateTime,
                tournamentId: tournamentId,
                userId: userId);

            return await _bulletinRepository.InsertAsync(bulletin);
        }
    }
}
