using Dapper;
using System.Data;
using TeamTactics.Application.Points;
using TeamTactics.Domain.Points;

namespace TeamTactics.Infrastructure.Database.Repositories;

class PointRepository(IDbConnection dbConnection) : IPointsRepository
{
    private IDbConnection _dbConnection = dbConnection;

    public async Task<IEnumerable<PointCategoryDto>> FindAllActiveAsync()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = $@"
    SELECT 
        name AS {nameof(PointCategoryDto.Name)}, 
        description AS {nameof(PointCategoryDto.Description)}, 
        point_amount AS {nameof(PointCategoryDto.PointAmount)}
    FROM team_tactics.point_category 
    WHERE active = true";

        var pointCategories = await _dbConnection.QueryAsync<PointCategoryDto>(sql);

        return pointCategories;
    }

    public Task<IEnumerable<PointCategory>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<PointCategory?> FindByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
}
