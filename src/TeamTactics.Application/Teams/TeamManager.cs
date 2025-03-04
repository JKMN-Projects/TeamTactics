
using TeamTactics.Domain.Teams;

namespace TeamTactics.Application.Teams
{
    public class TeamManager
    {
        private readonly ITeamRepository _teamRepository;

        public TeamManager(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<int> CreateTeamAsync(string name, int userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            var team = new Team(name, userId);
            return await _teamRepository.InsertAsync(team);
        }
    }
}
