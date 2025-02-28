namespace TeamTactics.Api.Requests.Users
{
    public sealed record RegisterUserRequest(string UserName, string Email, string Password);
}
