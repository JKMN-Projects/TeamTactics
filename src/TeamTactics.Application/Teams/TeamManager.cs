
using Microsoft.Extensions.Logging;
using TeamTactics.Application.Players;
using TeamTactics.Domain.Teams;

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

        public async Task<int> CreateTeamAsync(string name, int userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            var team = new Team(name, userId);
            return await _teamRepository.InsertAsync(team);
        }

        public async Task AddPlayerToTeam(int teamId, int playerId)
        {
            var player = await _playerRepository.FindById(playerId);
            if (player == null)
            {
                throw new InvalidOperationException("Player not found"); // TODO: Throw not found exception
            }

            var team = await _teamRepository.FindById(teamId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found"); // TODO: Throw not found exception
            }

            team.AddPlayer(player);
            await _teamRepository.UpdateAsync(team);
        }
    }
}
