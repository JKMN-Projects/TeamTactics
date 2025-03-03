using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Players;

namespace TeamTactics.Application.Players;

public interface IPlayerRepository : IRepository<Player, int>
{
    public Task<IEnumerable<PlayerDto>> GetPlayersAsync(int? competitionId = null);
}
