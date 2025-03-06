using Dapper;
using System.Data;
using TeamTactics.Application.Competitions;
using TeamTactics.Domain.Competitions;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class CompetitionRepository(IDbConnection dbConnection) : ICompetitionRepository
{
    private IDbConnection _dbConnection = dbConnection;

    public async Task<IEnumerable<Competition>> FindAllAsync()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT id, name, start_date, end_date FROM team_tactics.competition";

        var competitions = await _dbConnection.QueryAsync<Competition>(sql);

        return competitions;
    }

    public async Task<Competition?> FindByIdAsync(int id)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        DynamicParameters parameters = new();
        parameters.Add("Id", id);

        string sql = $@"
    SELECT 
        id as {nameof(Competition.Id)}, 
        name as {nameof(Competition.Name)}, 
        start_date as {nameof(Competition.StartDate)}, 
        end_date as {nameof(Competition.EndDate)}
    FROM team_tactics.competition
    WHERE id = @Id";

        var competition = await _dbConnection.QuerySingleOrDefaultAsync<Competition?>(sql, parameters);

        return competition;
    }
}
