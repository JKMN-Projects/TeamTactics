using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Tournaments
{
    public sealed record JoinTournamentRequest(
        [Required] string InviteCode,
        [Required] string TeamName);
}
