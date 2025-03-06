
using TeamTactics.Application.Competitions;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Tournaments;

namespace TeamTactics.Application.Tournaments
{
    public sealed class TournamentManager
    {
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ICompetitionRepository _competitionRepository;

        public TournamentManager(ITournamentRepository tournamentRepository, ICompetitionRepository competitionRepository)
        {
            _tournamentRepository = tournamentRepository;
            _competitionRepository = competitionRepository;
        }

        public async Task<int> CreateTournamentAsync(string name, int competitionId, int createdByUserId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentOutOfRangeException.ThrowIfLessThan(competitionId, 1, nameof(competitionId));
            ArgumentOutOfRangeException.ThrowIfLessThan(createdByUserId, 1, nameof(createdByUserId));

            // Check if competition exists
            var competition = await _competitionRepository.FindByIdAsync(competitionId);
            if (competition == null)
            {
                throw EntityNotFoundException.ForEntity<Competition>(competitionId, nameof(Competition.Id));
            }

            var tournament = new Tournament(name, competitionId, createdByUserId);
            await _tournamentRepository.InsertAsync(tournament);
            return tournament.Id;
        }

        public async Task DeleteTournamentAsync(int tournamentId, int userId)
        {
            var tournament = await _tournamentRepository.FindByIdAsync(tournamentId);
            if (tournament == null)
            {
                throw EntityNotFoundException.ForEntity<Tournament>(tournamentId, nameof(Tournament.Id));
            }

            // TODO: Could be moved to specific authorization service
            if (tournament.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException("User is not authorized to delete this tournament.");
            }

            await _tournamentRepository.RemoveAsync(tournamentId);
        }
    }
}
