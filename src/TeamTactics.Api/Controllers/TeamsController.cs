using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeamTactics.Api.Requests.Teams;
using TeamTactics.Application.Teams;

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
                ?? throw new Exception("Unauthorized")); // TODO: Throw unauthorized exception
            int teamId = await _teamManager.CreateTeamAsync(request.Name, userId);
            return Created(); // TODO: Return CreatedAtAction
            //return CreatedAtAction(nameof(GetTeamAsync), new { id = teamId });
        }

        [HttpPut("{id:int}/players/add")]
        [Authorize]
        public async Task<IActionResult> AddPlayerToTeam(int id, [FromBody] AddPlayerToTeamRequest request)
        {
            await _teamManager.AddPlayerToTeam(id, request.PlayerId);
            return NoContent();
        }
    }
}
