using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Tournaments
{
    public sealed record UpdateTournamentRequest(
        [Required] string Name,
        [Required] string Description);
}
