using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Common;

namespace TeamTactics.Application.Users;

public class ProfileDto : Entity
{
    public string Username;
    public string Email;
    public List<UserTournamentTeamDto> Tournaments;

    public ProfileDto(int id, string username, string email) : base(id)
    {
        Username = username;
        Email = email;
        Tournaments = new List<UserTournamentTeamDto>();
    }
}
