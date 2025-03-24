using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamTactics.Application.Points;

namespace TeamTactics.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class PointsController : ControllerBase
{
    private readonly PointsManager _pointsManager;

    public PointsController(PointsManager pointsManager)
    {
        _pointsManager = pointsManager;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var pointCategories = await _pointsManager.GetActivePointCategoriesAsync();
        return Ok(pointCategories);
    }

    [HttpGet("matches/{matchId}")]
    [Authorize]
    [ProducesResponseType<IEnumerable<PointResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatchPoints(int matchId)
    {
        var points = await _pointsManager.GetMatchPoints(matchId);
        return Ok(points);
    }
}
