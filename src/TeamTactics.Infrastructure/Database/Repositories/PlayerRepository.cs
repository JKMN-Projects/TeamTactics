using TeamTactics.Application.Players;
using TeamTactics.Domain.Players;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class PlayerRepository : IPlayerRepository
{
    public Task<IEnumerable<Player>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Player> FindById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PlayerDto>> GetPlayersAsync(int? competitionId = null)
    {
        throw new NotImplementedException();
    }
}
