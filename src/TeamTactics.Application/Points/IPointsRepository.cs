
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Teams;
using TeamTactics.Domain.Points;

namespace TeamTactics.Application.Points;

public interface IPointsRepository : IRepository<PointCategory, int>
{
    public Task<IEnumerable<PointCategoryDto>> FindAllActiveAsync();

    public Task<TeamPointsDto> FindTeamPointsAsync(int teamId);
    Task<IEnumerable<PointResultDto>> GetPointResultFromMatchIdAsync(int matchId);
    Task<IEnumerable<PointResultDto>> GetPointResultFromTeamIdAsync(int teamId);
}
