using Dapper;
using System.Data;
using TeamTactics.Application.Teams;
using TeamTactics.Domain.Teams;

namespace TeamTactics.Infrastructure.Database.Repositories
{
    class TeamRepository(IDbConnection dbConnection) : ITeamRepository
    {
        private IDbConnection _dbConnection = dbConnection;

        public Task<IEnumerable<Team>> FindAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Team?> FindByIdAsync(int id)
        {

            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            string sql = @"SELECT * FROM team_tactics.user_team
	WHERE id = @Id";

            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            Team? team = await _dbConnection.QuerySingleOrDefaultAsync<Team?>(sql, parameters);

            return team;
        }

        public async Task<TeamPointsDto> FindTeamPointsAsync(int teamId)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            string sql = @"
    SELECT SUM(pc.point_amount * mpp.occurrences)
    FROM team_tactics.player_user_team put
    JOIN team_tactics.match_player_point mpp 
    	ON put.player_id = mpp.player_id
    JOIN team_tactics.point_category pc 
    	ON mpp.point_category_id = pc.id
    WHERE put.user_team_id = @TeamId
        AND pc.active = true";

            var parameters = new DynamicParameters();
            parameters.Add("TeamId", teamId);

            var categoryTotals = await _dbConnection.QueryAsync<decimal>(sql, parameters);
            decimal totalPoints = categoryTotals.Sum();

            return new TeamPointsDto(totalPoints);
        }

        public Task<IEnumerable<Team>> FindUserTeamsAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync(Team model)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveAsync(int id)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            //ON DELETE CASCADE deletes all player_user_team associated with the team
            string sql = @"DELETE FROM team_tactics.user_team
	WHERE id = @Id";

            await _dbConnection.ExecuteAsync(sql, parameters);
        }

        public async Task UpdateAsync(Team team)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            string sql = @"
    UPDATE team_tactics.user_team 
    SET name = @Name, 
        status = @Status
    WHERE id = @Id";

            var parameters = new DynamicParameters();
            parameters.Add("Id", team.Id);
            parameters.Add("Name", team.Name);
            parameters.Add("Status", team.Status);

            await _dbConnection.ExecuteAsync(sql, parameters);
        }
    }
}
