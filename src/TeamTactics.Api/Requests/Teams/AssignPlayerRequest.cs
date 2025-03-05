using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Teams
{
    public record AssignPlayerRequest(
        [Required] int PlayerId);
}
