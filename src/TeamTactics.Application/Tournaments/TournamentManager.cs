
using TeamTactics.Application.Competitions;
using TeamTactics.Application.Points;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Tournaments;

namespace TeamTactics.Application.Tournaments
{
    public sealed class TournamentManager
    {
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ICompetitionRepository _competitionRepository;
        private readonly IPointsRepository _pointsRepository;

        public TournamentManager(ITournamentRepository tournamentRepository, ICompetitionRepository competitionRepository, IPointsRepository pointsRepository)
        {
            _tournamentRepository = tournamentRepository;
            _competitionRepository = competitionRepository;
            _pointsRepository = pointsRepository;
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

        /// <summary>
        /// Update the name and description of a tournament.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tournamentId"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="UnauthorizedException"></exception>
        public async Task UpdateTournamentAsync(int userId, int tournamentId, string name, string description)
        {
            Tournament? tournament = await _tournamentRepository.FindByIdAsync(tournamentId);
            if (tournament == null)
            {
                throw EntityNotFoundException.ForEntity<Tournament>(tournamentId, nameof(Tournament.Id));
            }

            if (tournament.CreatedByUserId != userId)
            {
                throw new UnauthorizedException("User is not authorized to update this tournament.");
            }

            tournament.Update(name, description);
            await _tournamentRepository.UpdateAsync(tournament);
        }

        public async Task<TournamentDetailsDto> GetTournamentDetails(int tournamentId)
        {
            return await _tournamentRepository.GetTournamentDetailsAsync(tournamentId);
        }

        public async Task<IEnumerable<TournamentTeamDto>> GetTournamentTeamsAsync(int tournamentId)
        {
            return await _tournamentRepository.GetTeamsInTournamentAsync(tournamentId);
        }
        
        public async Task<IEnumerable<UserTournamentTeamDto>> GetTournamentTeamsByUser(int userId)
        {
            var tournaments = await _tournamentRepository.GetJoinedTournamentsAsync(userId);
            foreach (var tournament in tournaments)
            {
                var teamPoints = await _pointsRepository.FindTeamPointsAsync(tournament.TeamId);
                tournament.TotalPoints = teamPoints.TotalPoints;
            }
            return tournaments;
        }
    }
}
