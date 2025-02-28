using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Users
{
    public sealed record RegisterUserRequest(
        [property:Required] string UserName,
        [property:Required] string Email,
        [property:Required] string Password);
}
