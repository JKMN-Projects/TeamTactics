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

        var competitionsResult = await _dbConnection.QueryAsync<(int id, string name, DateOnly startDate, DateOnly endDate)>(sql);

        return competitionsResult.Any() ? competitionsResult.Select(c => new Competition(c.id, c.name, c.startDate, c.endDate)) : new List<Competition>();
    }

    public async Task<Competition?> FindByIdAsync(int id)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        DynamicParameters parameters = new();
        parameters.Add("Id", id);

        string sql = $@"SELECT id, name, start_date, end_date FROM team_tactics.competition 
                        WHERE id = @Id";

        var competitionsResult = await _dbConnection.QuerySingleOrDefaultAsync<(int id, string name, DateOnly startDate, DateOnly endDate)?>(sql, parameters);

        return competitionsResult.HasValue ? new Competition(competitionsResult.Value.id, competitionsResult.Value.name, competitionsResult.Value.startDate, competitionsResult.Value.endDate) : null;
    }
}
