using TeamTactics.Application.Players;
using TeamTactics.Domain.Players;
using Dapper;
using System.Data.Common;
using System.Data;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class PlayerRepository(IDbConnection dbConnection) : IPlayerRepository
{
    private IDbConnection _dbConnection = dbConnection;
    
    public Task<IEnumerable<Player>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Player> FindByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PlayerDto>> GetPlayersAsync(int? competitionId = null)
    {
        throw new NotImplementedException();
    }
}
