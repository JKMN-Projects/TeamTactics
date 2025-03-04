
using Microsoft.Extensions.Logging;
using TeamTactics.Application.Players;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Teams.Exceptions;

namespace TeamTactics.Application.Teams
{
    public class TeamManager
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<TeamManager> _logger;

        public TeamManager(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            ILogger<TeamManager> logger)
        {
            _teamRepository = teamRepository;
            _playerRepository = playerRepository;
            _logger = logger;
        }

        public async Task<int> CreateTeamAsync(string name, int userId, int competitionId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentOutOfRangeException.ThrowIfLessThan(userId, 1, nameof(userId));
            ArgumentOutOfRangeException.ThrowIfLessThan(competitionId, 1, nameof(competitionId));

            // TODO: Check if competition exists, might be checked on foreign key constraint

            var team = new Team(name, userId, competitionId);
            return await _teamRepository.InsertAsync(team);
        }
        public async Task<TeamPointsDto> GetTeamPointsAsync(int teamId)
        {
            var team = await _teamRepository.FindTeamPointsAsync(teamId);
            return team;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException" />
        /// <exception cref="TeamLockedException"></exception>
        /// <exception cref="PlayerAlreadyInTeamException"></exception>
        /// <exception cref="TeamFullException"></exception>
        /// <exception cref="MaximumPlayersFromSameClubReachedException"></exception>
        public async Task AddPlayerToTeam(int teamId, int playerId)
        {
            var player = await _playerRepository.FindById(playerId);
            if (player == null)
            {
                throw EntityNotFoundException.ForEntity<Player>(playerId, nameof(Player.Id));
            }

            var team = await _teamRepository.FindById(teamId);
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
        /// Remove a player from a team.
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="TeamLockedException"></exception>
        /// <exception cref="PlayerNotOnTeamException"></exception>
        public async Task RemovePlayerFromTeam(int teamId, int playerId)
        {
            var team = await _teamRepository.FindById(teamId);
            if (team == null)
            {
                throw EntityNotFoundException.ForEntity<Team>(teamId, nameof(Team.Id));
            }
         
            team.RemovePlayer(playerId);
            
            await _teamRepository.UpdateAsync(team);
            _logger.LogInformation("Player '{playerId}' removed from team '{teamId}'", playerId, teamId);
        }
    }
}
