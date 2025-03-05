using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamTactics.Application.Players;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Users;

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
