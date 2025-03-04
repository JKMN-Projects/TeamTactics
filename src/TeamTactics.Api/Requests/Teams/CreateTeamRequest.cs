using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Teams
{
    public record CreateTeamRequest(
        [property:Required] string Name);
}
