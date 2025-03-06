using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Teams
{
    public record CreateTeamRequest(
        [Required] string Name,
        [Required] int competitionId);
}
