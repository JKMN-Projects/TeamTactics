using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Teams
{
    public record AddPlayerToTeamRequest(
        [property: Required] int TeamId,
        [property: Required] int PlayerId);
}
