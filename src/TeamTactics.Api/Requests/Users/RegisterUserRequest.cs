using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Users
{
    public sealed record RegisterUserRequest(
        [Required] string Username,
        [Required, EmailAddress] string Email,
        [Required] string Password);
}
