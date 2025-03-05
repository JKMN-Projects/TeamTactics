using System.ComponentModel.DataAnnotations;

namespace TeamTactics.Api.Requests.Authentication
{
    public record LoginRequest(
        [Required] string Email, 
        [Required] string Password);
}
