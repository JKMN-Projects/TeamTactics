using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Tournaments
{
    public record CreateBulletinRequest(
        [Required] string Text);
}
