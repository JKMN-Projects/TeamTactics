using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeamTactics.Api.Requests.Teams;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Teams;
using TeamTactics.Domain.Teams.Exceptions;
using TeamTactics.Domain.Tournaments.Exceptions;

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
        [ProducesResponseType<int>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedException("User not logged in."));
            try
            {
                int teamId = await _teamManager.CreateTeamAsync(request.Name, userId, request.InviteCode);
                return Created("", teamId);
            }
            catch (AlreadyJoinedTournamentException ex)
            {
                return Problem(
                    title: "User already joined the tournament.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status409Conflict);
            }
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
        public async Task<IActionResult> AssignPlayer(int id, [FromBody] AssignPlayerRequest request)
        {
            try
            {
                await _teamManager.AddPlayerToTeamAsync(id, request.PlayerId);
                return NoContent();
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
        }

        [HttpPut("{teamId:int}/players/{playerId:int}/remove")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemovePlayerFromTeam(int teamId, int playerId)
        {
            try
            {
                await _teamManager.RemovePlayerFromTeamAsync(teamId, playerId);
                return NoContent();
            }
            catch (TeamLockedException ex)
            {
                return Problem(
                    title: "Team is locked.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status400BadRequest);
            }
            catch (PlayerNotOnTeamException ex)
            {
                return Problem(
                    title: "Player is not on the team.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status404NotFound);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            await _teamManager.DeleteTeamAsync(id);
            return NoContent();
        }

        [HttpPut("{id:int}/players/{playerId:int}/set-captain")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SetCaptain(int id, int playerId)
        {
            try
            {
                await _teamManager.SetTeamCaptain(id, playerId);
                return NoContent();
            }
            catch (TeamLockedException ex)
            {
                return Problem(
                    title: "Team is locked.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status400BadRequest);
            }
            catch (PlayerNotOnTeamException ex)
            {
                return Problem(
                    title: "Player is not on the team.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status404NotFound);
            }
            catch (PlayerAlreadyCaptainException ex)
            {
                return Problem(
                    title: "Player is already the captain.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpPatch("{id:int}/lock")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LockTeam(int id)
        {
            try
            {
                await _teamManager.LockTeamAsync(id);
                return NoContent();
            }
            catch (TeamLockedException ex)
            {
                return Problem(
                    title: "Team is already locked.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status400BadRequest);
            }
            catch (TeamNotFullException ex)
            {
                return Problem(
                    title: "The team is not full.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status400BadRequest);
            }
            catch (NoCaptainException ex)
            {
                return Problem(
                    title: "The team does not have a captain.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }
    }
}
