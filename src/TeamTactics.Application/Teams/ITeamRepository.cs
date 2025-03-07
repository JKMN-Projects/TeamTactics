
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Teams;

namespace TeamTactics.Application.Teams
{
    public interface ITeamRepository : ICrudRepository<Team, int>
    {
        Task<IEnumerable<TeamTournamentsDto>> GetAllTeamsByUserId(int userId);
    }
}
