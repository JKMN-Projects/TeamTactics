using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TeamTactics.Api.Requests.Users;
using TeamTactics.Application.Tournaments;
using TeamTactics.Application.Users;

namespace TeamTactics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager _userManager;
        private readonly TournamentManager _tournamentManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager userManager, ILogger<UsersController> logger, TournamentManager tournamentManager)
        {
            _userManager = userManager;
            _logger = logger;
            _tournamentManager = tournamentManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserRequest request)
        {
            var safeResult = Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Username, email or password was incorrect."
                );

            try
            {
                await _userManager.CreateUserAsync(request.Username, request.Email, request.Password);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to create user");
                return safeResult;
            }
            catch (PostgresException ex)
            {
                _logger.LogError(ex, "Failed to create user");
                return safeResult;
            }
        }

        [HttpGet("{userId}/Profile")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var user = await _userManager.GetProfileAsync(userId);
            return Ok(user);
        }

        [HttpGet("{userId}/tournaments/teams")]
        [Authorize]
        [ProducesResponseType<UserTournamentTeamDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserTournamentTeams(int userId)
        {
            var tournaments = await _tournamentManager.GetTournamentTeamsByUser(userId);
            return Ok(tournaments);
        }
    }
}