using Microsoft.AspNetCore.Mvc;
using TeamTactics.Api.Requests.Authentication;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Common.Models;
using TeamTactics.Application.Users;

namespace TeamTactics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager _userManager;

        public AuthenticationController(UserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var problemResult = (string? reason = null) => Problem(
                title: reason ?? "Email or password incorrect.",
                statusCode: StatusCodes.Status401Unauthorized);
            try
            {
                AuthenticationToken token = await _userManager.GetAuthenticationTokenAsync(request.Email, request.Password);
                return Ok(token);
            }
            catch (UnauthorizedException)
            {
                return problemResult();
            }
            catch (EntityNotFoundException)
            {
                return problemResult();
            }
            catch (ArgumentException)
            {
                return problemResult();
            }
        }
    }
}
