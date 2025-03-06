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

    public Task<Competition?> FindByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

}
