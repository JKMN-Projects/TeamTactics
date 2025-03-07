
using Microsoft.Extensions.Logging;
using TeamTactics.Application.Players;
using TeamTactics.Application.Points;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Teams.Exceptions;

namespace TeamTactics.Application.Teams
{
    public sealed class TeamManager
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IPointsRepository _pointRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<TeamManager> _logger;

        public TeamManager(
            ITeamRepository teamRepository,
            IPointsRepository pointRepository,
            IPlayerRepository playerRepository,
            ILogger<TeamManager> logger)
        {
            _teamRepository = teamRepository;
            _pointRepository = pointRepository;
            _playerRepository = playerRepository;
            _logger = logger;
        }

        /// <summary>
        /// Create a new team with the given name, user and competition.
        /// </summary>
        /// <param name="name">The name of the team. Must be unique in the tournament.</param>
        /// <param name="userId">The user id of the user creating the team.</param>
        /// <param name="competitionId">The id of the competition the team is based on.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        public async Task<int> CreateTeamAsync(string name, int userId, int competitionId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentOutOfRangeException.ThrowIfLessThan(userId, 1, nameof(userId));
            ArgumentOutOfRangeException.ThrowIfLessThan(competitionId, 1, nameof(competitionId));

            // TODO: Check if competition exists, might be checked on foreign key constraint

            var team = new Team(name, userId, competitionId);
            return await _teamRepository.InsertAsync(team);
        }
        

        /// <summary>
        /// Add player to team if both player and team exists. Throws <see cref="EntityNotFoundException"/> if either player or team does not exist."/>
        /// </summary>
        /// <param name="teamId">The id of the team to add the player to.</param>
        /// <param name="playerId">The id of the player to be added to the team.</param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException" />
        /// <exception cref="TeamLockedException"></exception>
        /// <exception cref="PlayerAlreadyInTeamException"></exception>
        /// <exception cref="TeamFullException"></exception>
        /// <exception cref="MaximumPlayersFromSameClubReachedException"></exception>
        public async Task AddPlayerToTeamAsync(int teamId, int playerId)
        {
            var player = await _playerRepository.FindByIdAsync(playerId);
            if (player == null)
            {
                throw EntityNotFoundException.ForEntity<Player>(playerId, nameof(Player.Id));
            }

            var team = await _teamRepository.FindByIdAsync(teamId);
            if (team == null)
            {
                throw EntityNotFoundException.ForEntity<Team>(teamId, nameof(Team.Id));
            }

            // TODO: Should check if the player is available to play in the competition matching the team. Might need the check inside the add player method in the domain.

            team.AddPlayer(player);

            await _teamRepository.UpdateAsync(team);
            _logger.LogInformation("Player '{playerId}' added to team '{teamId}'", playerId, teamId);
        }

        /// <summary>
        /// Delete a team.
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException" />
        public async Task DeleteTeamAsync(int teamId)
        {
            var team = await _teamRepository.FindByIdAsync(teamId);
            if (team == null)
            {
                throw EntityNotFoundException.ForEntity<Team>(teamId, nameof(Team.Id));
            }

            await _teamRepository.RemoveAsync(teamId);
            _logger.LogInformation("Team '{teamId}' deleted", teamId);
        }

        public async Task<TeamPointsDto> GetTeamPointsAsync(int teamId)
        {
            var teamPoints = await _pointRepository.FindTeamPointsAsync(teamId);
            return teamPoints;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="TeamLockedException"></exception>
        /// <exception cref="TeamNotFullException"></exception>
        /// <exception cref="NoCaptainException"></exception>
        public async Task LockTeamAsync(int teamId)
        {
            var team = await _teamRepository.FindByIdAsync(teamId);
            if (team == null)
            {
                throw EntityNotFoundException.ForEntity<Team>(teamId, nameof(Team.Id));
            }

            team.Lock();

            await _teamRepository.UpdateAsync(team);
            _logger.LogInformation("Team '{teamId}' locked", teamId);
        }

        /// <summary>
        /// Remove a player from a team.
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="TeamLockedException"></exception>
        /// <exception cref="PlayerNotOnTeamException"></exception>
        public async Task RemovePlayerFromTeamAsync(int teamId, int playerId)
        {
            var team = await _teamRepository.FindByIdAsync(teamId);
            if (team == null)
            {
                throw EntityNotFoundException.ForEntity<Team>(teamId, nameof(Team.Id));
            }
         
            team.RemovePlayer(playerId);
            
            await _teamRepository.UpdateAsync(team);
            _logger.LogInformation("Player '{playerId}' removed from team '{teamId}'", playerId, teamId);
        }

        /// <summary>
        /// Set the team captain to the player with the given id.
        /// Throws <see cref="EntityNotFoundException"/> if either team does not exist.
        /// </summary>
        /// <param name="teamId">The id of the team to change the id.</param>
        /// <param name="playerId">The id of the player.</param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException" />
        /// <exception cref="TeamLockedException"></exception>
        /// <exception cref="PlayerNotOnTeamException"></exception>
        /// <exception cref="PlayerAlreadyCaptainException"></exception>
        public async Task SetTeamCaptain(int teamId, int playerId)
        {
            var team = await _teamRepository.FindByIdAsync(teamId);
            if (team == null)
            {
                throw EntityNotFoundException.ForEntity<Team>(teamId, nameof(Team.Id));
            }

            team.SetCaptain(playerId);
            await _teamRepository.UpdateAsync(team);
            _logger.LogInformation("Player '{playerId}' set as captain of team '{teamId}'", playerId, teamId);
        }
    }
}
