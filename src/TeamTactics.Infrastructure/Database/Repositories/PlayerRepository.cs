using Dapper;
using System.Data;
using TeamTactics.Application.Players;
using TeamTactics.Domain.Players;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class PlayerRepository(IDbConnection dbConnection) : IPlayerRepository
{
    private IDbConnection _dbConnection = dbConnection;
    
    public Task<IEnumerable<Player>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Player?> FindByIdAsync(int id)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"SELECT * FROM team_tactics.player
	WHERE id = @Id";

        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        Player? player = await _dbConnection.QuerySingleOrDefaultAsync<Player?>(sql, parameters);

        return player;
    }

    public async Task<IEnumerable<PlayerDto>> GetPlayersAsync(int? competitionId = null)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @"
    SELECT 
        p.id AS ""Id"",
        p.first_name AS ""FirstName"",
        p.last_name AS ""LastName"",
        c.id AS ""ClubId"",
        c.name AS ""ClubName"",
        pp.id AS ""PositionId"",
        pp.name AS ""PositionName""
    FROM team_tactics.player p
    JOIN team_tactics.player_contract pc 
        ON p.id = pc.player_id AND pc.active = true
    JOIN team_tactics.club c 
        ON c.id = pc.club_id
    JOIN team_tactics.player_position pp 
        ON pp.id = p.position_id";

        // Add competition filtering if provided
        if (competitionId.HasValue)
        {
            sql += @"
        JOIN team_tactics.club_competition cc 
            ON c.id = cc.club_id
        WHERE cc.competition_id = @CompetitionId";
        }

        var parameters = new DynamicParameters();
        if (competitionId.HasValue)
        {
            parameters.Add("CompetitionId", competitionId.Value);
        }

        return await _dbConnection.QueryAsync<PlayerDto>(sql, parameters,
            commandType: CommandType.Text);
    }
}
