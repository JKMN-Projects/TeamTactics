using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            return Created();
            //return CreatedAtAction(nameof(GetTournamentAsync), new { id = tournamentId });
        }
    }
}
