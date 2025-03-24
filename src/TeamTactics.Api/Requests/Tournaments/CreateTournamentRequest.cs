using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Tournaments
{
    public sealed record CreateTournamentRequest(
        [Required] string Name,
        [Required] string Description,
        [Required] string TeamName,
        [Required] int CompetitionId);
}
