using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Teams
{
    public sealed record AssignFormationRequest(
        [Required] string Name);
}
