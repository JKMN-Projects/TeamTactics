using TeamTactics.Application.Players;

namespace TeamTactics.Infrastructure.Database.Repositories;

internal class PlayerRepository : IPlayerRepository
{
    public Task<IEnumerable<PlayerDto>> GetPlayersAsync(int? competitionId = null)
    {
        throw new NotImplementedException();
    }
}
