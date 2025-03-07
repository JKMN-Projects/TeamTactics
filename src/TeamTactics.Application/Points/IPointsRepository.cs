
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Points;

namespace TeamTactics.Application.Points;

public interface IPointsRepository : IRepository<PointCategory, int>
{
    public Task<IEnumerable<PointCategoryDto>> FindAllActiveAsync();
}
