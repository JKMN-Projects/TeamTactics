
using Microsoft.Extensions.Logging;
using TeamTactics.Application.Players;
using TeamTactics.Application.Points;
using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams;
using TeamTactics.Domain.Teams.Exceptions;
using TeamTactics.Domain.Tournaments;
using TeamTactics.Domain.Tournaments.Exceptions;

namespace TeamTactics.Application.Teams
{
    public sealed class TeamManager
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IPointsRepository _pointRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ILogger<TeamManager> _logger;

        public TeamManager(
            ITeamRepository teamRepository,
            IPointsRepository pointRepository,
            IPlayerRepository playerRepository,
            ITournamentRepository tournamentRepository,
            ILogger<TeamManager> logger)
        {
            _teamRepository = teamRepository;
            _pointRepository = pointRepository;
            _playerRepository = playerRepository;
            _tournamentRepository = tournamentRepository;
            _logger = logger;
        }

        /// <summary>
        /// Used to return all players on a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TeamPlayerDto>> GetTeamPlayersAsync(int teamId)
        {
            var teamPlayers = await _teamRepository.GetTeamPlayersByTeamIdAsync(teamId);
            return teamPlayers;
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

        public async Task<IEnumerable<PointResultDto>> GetTeamPointsAsync(int teamId)
        {
            var teamPoints = await _pointRepository.GetPointResultFromTeamIdAsync(teamId);
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
        public async Task SetTeamCaptainAsync(int teamId, int playerId)
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

        /// <summary>
        /// Rename a team.
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException" />
        /// <exception cref="TeamLockedException"></exception>
        public async Task RenameTeamAsync(int userId, int teamId, string name)
        {
            var team = await _teamRepository.FindByIdAsync(teamId);
            if (team == null)
            {
                throw EntityNotFoundException.ForEntity<Team>(teamId, nameof(Team.Id));
            }

            if (team.UserId != userId)
            {
                throw new UnauthorizedException("User does not own the team.");
            }

            team.Rename(name);

            await _teamRepository.UpdateAsync(team);
            _logger.LogInformation("Team '{teamId}' renamed to '{name}'", teamId, name);
        }

        /// <summary>
        /// Get a team by id.
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException" />"
        public async Task<TeamDto> GetTeamById(int teamId)
        {
            var team = await _teamRepository.GetTeamDtoByIdAsync(teamId);
            if (team == null)
            {
                throw EntityNotFoundException.ForEntity<Team>(teamId, nameof(Team.Id));
            }
            return team;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="formation"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>"
        /// <exception cref="TeamLockedException"></exception>"
        public async Task AssignFormation(int teamId, string formation)
        {
            Team? team = await _teamRepository.FindByIdAsync(teamId);
            if (team == null)
            {
                throw EntityNotFoundException.ForEntity<Team>(teamId, nameof(Team.Id));
            }

            team.SetFormation(formation);
            await _teamRepository.UpdateAsync(team);
            _logger.LogInformation("Formation '{formation}' assigned to team '{teamId}'", formation, teamId);
        }
    }
}