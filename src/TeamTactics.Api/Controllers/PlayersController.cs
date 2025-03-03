using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamTactics.Application.Players;

namespace TeamTactics.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayersController : ControllerBase
{
    private readonly PlayerManager _playerManager;
    public PlayersController(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }
    [HttpGet("Players/{competitionId?}")]
    [ProducesResponseType<PlayerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayers([FromQuery]int? competitionId = null)
    {
        var players = await _playerManager.GetPlayersAsync(competitionId);
        return Ok(players);
    }
}
