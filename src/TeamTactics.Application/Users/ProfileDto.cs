namespace TeamTactics.Application.Users;

public record ProfileDto(int userId, string Username, string Email)
{
    public record Tournament(int Id, string Name);
}
