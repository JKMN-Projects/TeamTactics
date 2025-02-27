﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamTactics.Api.Requests.Users;
using TeamTactics.Application.Users;

namespace TeamTactics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager _userManager;

        public UsersController(UserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateUserAsync([FromBody] RegisterUserRequest request)
        {
            await _userManager.CreateUserAsync(request.UserName, request.Email, request.Password);
            return Ok();
        }
    }
}