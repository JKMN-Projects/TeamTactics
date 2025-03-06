using Dapper;
using System.Data;
using TeamTactics.Domain.Competitions;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class CompetitionRepository(IDbConnection dbConnection)
{
    private IDbConnection _dbConnection = dbConnection;

    public async Task<IEnumerable<Competition>> GetAllCompetitionsAsync()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT id, name, start_date, end_date FROM team_tactics.competitions";

        var competitions = await _dbConnection.QueryAsync<Competition>(sql);

        return competitions;
    }
}
