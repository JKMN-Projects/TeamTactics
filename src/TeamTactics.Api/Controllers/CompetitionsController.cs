using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamTactics.Application.Competitions;

namespace TeamTactics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionsController : ControllerBase
    {
        private readonly CompetitionManager _competitionManager;

        public CompetitionsController(CompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompetitions()
        {
            var competitions = await _competitionManager.GetAllCompetitionsAsync();
            return Ok(competitions);
        }
    }
}
