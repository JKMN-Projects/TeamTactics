﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeamTactics.Api.Requests.Tournaments;
using TeamTactics.Application.Bulletins;
using TeamTactics.Application.Common.Exceptions;
using TeamTactics.Application.Matches;
using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Tournaments.Exceptions;

namespace TeamTactics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentsController : ControllerBase
    {
        private readonly TournamentManager _tournamentManager;
        private readonly MatchManager _matchManager;
        private readonly BulletinManager _bulletinManager;

        public TournamentsController(TournamentManager tournamentManager, MatchManager matchManager, BulletinManager bulletinManager)
        {
            _tournamentManager = tournamentManager;
            _matchManager = matchManager;
            _bulletinManager = bulletinManager;
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
            int tournamentId = await _tournamentManager.CreateTournamentAsync(
                request.Name,
                request.TeamName,
                request.CompetitionId,
                userId);
            return CreatedAtAction(nameof(GetTournamentDetails), new { id = tournamentId }, new { id = tournamentId });
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
            try
            {
                int joinedTeamId = await _tournamentManager.JoinTournamentAsync(userId, request.InviteCode, request.TeamName);
                return Ok(joinedTeamId);
            }
            catch (AlreadyJoinedTournamentException ex)
            {
                return Problem(
                    title: "User already joined the tournament.",
                    detail: ex.Description,
                    statusCode: StatusCodes.Status409Conflict);
            }
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
        [ProducesResponseType<IEnumerable<TournamentTeamDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTournamentTeams(int id)
        {
            var teams = await _tournamentManager.GetTournamentTeamsAsync(id);
            return Ok(teams);
        }

        [HttpGet("{id}/matches")]
        [Authorize]
        [ProducesResponseType<IEnumerable<MatchDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTournamentMatches(int id)
        {
            var matches = await _matchManager.GetTournamentMatches(id);
            return Ok(matches);
        }

        [HttpPost("{id}/create-bulletin")]
        [Authorize]
        [ProducesResponseType<int>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateBulletin(int id, [FromBody] CreateBulletinRequest request)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedException("User not logged in."));
            int bulletinId = await _bulletinManager.CreateBulletinAsync(request.Text, id, userId);
            return Ok(bulletinId);
        }

        [HttpGet("{id}/bulletins")]
        [Authorize]
        [ProducesResponseType<IEnumerable<BulletinDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBulletins(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedException("User not logged in."));
            var bulletins = await _bulletinManager.GetBulletinsForTournamentAsync(userId, id);
            return Ok(bulletins);
        }
    }
}
