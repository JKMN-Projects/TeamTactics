using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamTactics.Application.Players;
using TeamTactics.Domain.Competitions;

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
    [HttpGet]
    [ProducesResponseType<PlayerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayers([FromQuery]int? competitionId = null)
    {
        var players = await _playerManager.GetPlayersAsync(competitionId);
        return Ok(players);
    }

    [HttpPost("StartClubPopulation/{externalCompetitionId}")]
    public async Task<IActionResult> StartClubPopulation(string externalCompetitionId)
    {
        await _playerManager.StartClubPopulation(externalCompetitionId);
        return Ok();
    }

    [HttpPost("StartPlayerPopulation/{competitionId}")]
    public async Task<IActionResult> StartPlayerPopulation(int competitionId)
    {
        await _playerManager.StartPlayerPopulation(competitionId);
        return Ok();
    }
    [HttpPost("LoadFixture/{competitionId}/{toDate}")]
    public async Task<IActionResult> StartPlayerPopulation(int competitionId, DateTime toDate)
    {
        await _playerManager.LoadFixtures(competitionId, toDate);
        return Ok();
    }
    [HttpPost("LoadPlayerStats/{competitionId}")]
    public async Task<IActionResult> LoadPlayerStats(int competitionId)
    {
        await _playerManager.LoadPlayerStatsForFixtures();
        return Ok();
    }
}
