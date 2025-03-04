
using System.Net.Http.Headers;

namespace TeamTactics.Application.Users;

public record ProfileDto(string Username, string FirstName, string LastName, string Email, List<ProfileDto.Competition> Competitions)
{
    public record Competition(int Id, string Name);

}
