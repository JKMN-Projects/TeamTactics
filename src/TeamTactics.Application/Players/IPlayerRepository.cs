namespace TeamTactics.Application.Players;

public interface IPlayerRepository
{
    public Task<IEnumerable<PlayerDto>> GetPlayersAsync(int? competitionId = null);
}
