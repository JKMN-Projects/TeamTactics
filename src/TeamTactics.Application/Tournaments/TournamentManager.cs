﻿
using TeamTactics.Application.Competitions;
using TeamTactics.Application.Teams;
using TeamTactics.Application.Points;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Tournaments;
using TeamTactics.Application.Matches;
using TeamTactics.Domain.Tournaments.Exceptions;

namespace TeamTactics.Application.Tournaments
{
    public sealed class TournamentManager
    {
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ICompetitionRepository _competitionRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IPointsRepository _pointsRepository;
        private readonly IMatchRepository _matchRepository;

        public TournamentManager(ITournamentRepository tournamentRepository, ICompetitionRepository competitionRepository, ITeamRepository teamRepository, IPointsRepository pointsRepository, IMatchRepository matchRepository)
        {
            _tournamentRepository = tournamentRepository;
            _competitionRepository = competitionRepository;
            _teamRepository = teamRepository;
            _pointsRepository = pointsRepository;
            _matchRepository = matchRepository;
        }

        /// <summary>
        /// Create a new tournament and a team for the user.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="teamName"></param>
        /// <param name="competitionId"></param>
        /// <param name="createdByUserId"></param>
        /// <returns>The id of the created tournament</returns>
        public async Task<int> CreateTournamentAsync(string name, string teamName, int competitionId, int createdByUserId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentException.ThrowIfNullOrWhiteSpace(teamName, nameof(teamName));
            ArgumentOutOfRangeException.ThrowIfLessThan(competitionId, 1, nameof(competitionId));
            ArgumentOutOfRangeException.ThrowIfLessThan(createdByUserId, 1, nameof(createdByUserId));

            // Check if competition exists
            var competition = await _competitionRepository.FindByIdAsync(competitionId);
            if (competition == null)
            {
                throw EntityNotFoundException.ForEntity<Competition>(competitionId, nameof(Competition.Id));
            }

            var tournament = new Tournament(name, createdByUserId, competitionId);
            await _tournamentRepository.InsertAsync(tournament);

            Team team = new Team(teamName, createdByUserId, tournament.Id);
            await _teamRepository.InsertAsync(team);

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

        /// <summary>
        /// Finds a tournament by its invite code and creates a new team for the user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="inviteCode"></param>
        /// <param name="teamName"></param>
        /// <returns>The id of the created team</returns>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task<int> JoinTournamentAsync(int userId, string inviteCode, string teamName)
        {
            int? tournamentId = await _tournamentRepository.FindIdByInviteCodeAsync(inviteCode);
            if (!tournamentId.HasValue)
            {
                throw EntityNotFoundException.ForEntity<Tournament>(inviteCode, nameof(Tournament.InviteCode));
            }

            bool isUserAlreadyMember = await _tournamentRepository.IsUserTournamentMember(userId, tournamentId.Value);
            if (isUserAlreadyMember)
            {
                throw new AlreadyJoinedTournamentException(userId, tournamentId.Value);
            }

            Team emptyTeam = new Team(teamName, userId, tournamentId.Value);
            int teamId = await _teamRepository.InsertAsync(emptyTeam);
            return teamId;
        }
        
        public async Task<TournamentDetailsDto> GetTournamentDetails(int tournamentId)
        {
            return await _tournamentRepository.GetTournamentDetailsAsync(tournamentId);
        }

        public async Task<IEnumerable<TournamentTeamDto>> GetTournamentTeamsAsync(int tournamentId)
        {
            var tournamentTeams = (await _tournamentRepository.GetTeamsInTournamentAsync(tournamentId)).ToList();

            foreach (var tournamentTeam in tournamentTeams)
            {
                var teamPoints = await _pointsRepository.FindTeamPointsAsync(tournamentTeam.TeamId);
                tournamentTeam.TotalPoints = teamPoints.TotalPoints;
            }

            return tournamentTeams;
        }
        
        public async Task<IEnumerable<UserTournamentTeamDto>> GetTournamentTeamsByUser(int userId)
        {
            var tournamentTeams = await _tournamentRepository.GetJoinedTournamentsAsync(userId);
            
            foreach (var tournamentTeam in tournamentTeams)
            {
                if(tournamentTeam.LockedDate == DateOnly.MinValue)
                {
                    continue;
                }
                var teamPoints = await _pointsRepository.FindTeamPointsAsync(tournamentTeam.TeamId);
                tournamentTeam.TotalPoints = teamPoints.TotalPoints;
            }
            return tournamentTeams;
        }
    }
}
