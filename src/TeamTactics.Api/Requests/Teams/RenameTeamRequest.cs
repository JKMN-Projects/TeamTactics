using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Teams
{
    public sealed record RenameTeamRequest(
        [Required] string Name);
}
