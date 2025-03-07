using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Application.Points;
using TeamTactics.Domain.Points;

namespace TeamTactics.Infrastructure.Database.Repositories
{
    class PointRepository(IDbConnection dbConnection) : IPointsRepository
    {
        private IDbConnection _dbConnection = dbConnection;

        public async Task<IEnumerable<PointCategoryDto>> FindAllActiveAsync()
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            string sql = @"SELECT name, description, point_amount FROM team_tactics.point_category WHERE active = true";

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
}
