namespace TeamTactics.Application.Users;

public record ProfileDto(string Username, string Email, List<ProfileDto.Tournament> Competitions)
{
    public record Tournament(int Id, string Name);
}
