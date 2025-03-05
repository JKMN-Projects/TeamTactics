using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeamTactics.Api.Requests.Teams;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Teams;
using TeamTactics.Domain.Teams.Exceptions;

namespace TeamTactics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly TeamManager _teamManager;

        public TeamsController(TeamManager teamManager)
        {
            _teamManager = teamManager;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedException("User not logged in."));
            int teamId = await _teamManager.CreateTeamAsync(request.Name, userId, request.competitionId);
            return Created(); // TODO: Return CreatedAtAction
            //return CreatedAtAction(nameof(GetTeamAsync), new { id = teamId });
        }

        [HttpGet("{teamId}/Points")]
        [Authorize]
        [ProducesResponseType<TeamPointsDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeamPoints(int teamId)
        {
            var team = await _teamManager.GetTeamPointsAsync(teamId);
            return Ok(team);
        }

        [HttpPut("{id:int}/players/add")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddPlayerToTeam(int id, [FromBody] AddPlayerToTeamRequest request)
        {
            try
            {
                await _teamManager.AddPlayerToTeamAsync(id, request.PlayerId);
            }
            catch (TeamLockedException ex)
            {
                return Problem(
                    title: "Team is locked.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status400BadRequest);
            }
            catch (PlayerAlreadyInTeamException ex)
            {
                return Problem(
                    title: "Player already added to the team.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status409Conflict);
            }
            catch (TeamFullException ex)
            {
                return Problem(
                    title: "The team is full.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status400BadRequest);
            }
            catch (MaximumPlayersFromSameClubReachedException ex)
            {
                return Problem(
                    title: "The team is full.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status400BadRequest);
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            await _teamManager.DeleteTeamAsync(id);
            return NoContent();
        }
    }
}
