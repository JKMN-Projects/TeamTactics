using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeamTactics.Api.Requests.Tournaments;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Tournaments;

namespace TeamTactics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentsController : ControllerBase
    {
        private readonly TournamentManager _tournamentManager;

        public TournamentsController(TournamentManager tournamentManager)
        {
            _tournamentManager = tournamentManager;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateTournament([FromBody] CreateTournamentRequest request)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedException("User not logged in."));
            int tournamentId = await _tournamentManager.CreateTournamentAsync(request.Name, request.CompetitionId, userId);
            return StatusCode(StatusCodes.Status201Created, new { id = tournamentId });
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTournament(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedException("User not logged in."));
            await _tournamentManager.DeleteTournamentAsync(id, userId);
            return NoContent();
        }

        [HttpPatch("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTournament(int id, [FromBody] UpdateTournamentRequest request)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedException("User not logged in."));

            await _tournamentManager.UpdateTournamentAsync(id, userId, request.Name, request.Description);
            return NoContent();
        }

        [HttpPost("join")]
        [Authorize]
        [ProducesResponseType<int>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> JoinTournament([FromBody] JoinTournamentRequest request)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedException("User not logged in."));
            int joinedTeamId = await _tournamentManager.JoinTournamentAsync(userId, request.InviteCode, request.TeamName);
            return Ok(joinedTeamId);
        }
        
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType<TournamentDetailsDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTournamentDetails(int id)
        {
            var tournament = await _tournamentManager.GetTournamentDetails(id);
            return Ok(tournament);
        }

        [HttpGet("{id}/teams")]
        [Authorize]
        [ProducesResponseType<IEnumerable<IEnumerable<TournamentTeamDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTournamentTeams(int id)
        {
            var teams = await _tournamentManager.GetTournamentTeamsAsync(id);
            return Ok(teams);
        }
    }
}
