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

        string sql = @"
SELECT 
    p.id,
    p.first_name,
    p.last_name,
    p.player_position_id AS posId,
    p.birthdate,
    p.external_id,
    pc.id,
    pc.club_id,
    pc.active
FROM team_tactics.player AS p 
LEFT JOIN team_tactics.player_contract AS pc 
	ON pc.player_id = p.id
WHERE p.id = @Id";

        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        var playerResult = await _dbConnection.QueryAsync<(
            int id, 
            string firstName, 
            string lastName, 
            int posId, 
            DateOnly birthdate,
            string externalId,
            int contractId,
            int clubId,
            bool contractActive)>(sql, parameters);

        if (!playerResult.Any())
        {
            return null;
        }

        // Get first row to create player
        var firstRow = playerResult.First();

        Player player = new Player(
            firstRow.id,
            firstRow.firstName,
            firstRow.lastName,
            firstRow.birthdate,
            firstRow.externalId,
            firstRow.posId);

        foreach (var playerContract in playerResult.OrderBy(x => x.contractActive ? 1 : 0))
        {
            if (playerContract.contractId != 0)
            {
                player.SignContract(playerContract.clubId);
            }
        }

        return player;
    }

    public async Task<IEnumerable<PlayerDto>> GetPlayersAsync(int? competitionId = null)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = $@"
    SELECT 
        p.id AS ""{nameof(PlayerDto.Id)}"",
        p.first_name AS ""{nameof(PlayerDto.FirstName)}"",
        p.last_name AS ""{nameof(PlayerDto.LastName)}"",
        c.id AS ""{nameof(PlayerDto.ClubId)}"",
        c.name AS ""{nameof(PlayerDto.ClubName)}"",
        pp.id AS ""{nameof(PlayerDto.PositionId)}"",
        pp.name AS ""{nameof(PlayerDto.PositionName)}""
    FROM team_tactics.player p
    JOIN team_tactics.player_contract pc 
        ON p.id = pc.player_id AND pc.active = true
    JOIN team_tactics.club c 
        ON c.id = pc.club_id
    JOIN team_tactics.player_position pp 
        ON pp.id = p.player_position_id
    ";

        // Add competition filtering if provided
        var parameters = new DynamicParameters();
        if (competitionId.HasValue)
        {
            sql += @"
            JOIN team_tactics.club_competition cc 
                ON c.id = cc.club_id
            WHERE cc.competition_id = @CompetitionId";

            parameters.Add("@CompetitionId", competitionId.Value);
        }

        var playerResult = await _dbConnection.QueryAsync<(int id, string firstName, string lastName, int clubId, string clubName, int posId, string posName)>(sql, parameters);

        return playerResult.Any() ? playerResult.Select(p => new PlayerDto(p.id, p.firstName, p.lastName, p.clubId, p.clubName, p.posId, p.posName)) : new List<PlayerDto>();
    }
}
