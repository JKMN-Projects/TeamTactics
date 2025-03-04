
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Teams;

namespace TeamTactics.Application.Teams
{
    public interface ITeamRepository : ICrudRepository<Team, int>
    {
        public Task<IEnumerable<Team>> FindUserTeamsAsync(int userId);
        public Task<TeamPointsDto> FindTeamPointsAsync(int teamId);
    }
}
