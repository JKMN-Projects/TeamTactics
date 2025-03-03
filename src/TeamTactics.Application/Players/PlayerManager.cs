using TeamTactics.Application.Users;

namespace TeamTactics.Application.Players;

public sealed class PlayerManager
{
    private readonly IPlayerRepository _playerRepository;
    public PlayerManager(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    //Queries
    //Queries use dto models
    public async Task<IEnumerable<PlayerDto>> GetPlayersAsync(int? competitionId = null)
    {
        return await _playerRepository.GetPlayersAsync(competitionId);
    }

    //Commands
    //Commands use domain models


}
