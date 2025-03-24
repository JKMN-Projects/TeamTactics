using TeamTactics.Application.Tournaments;
using TeamTactics.Domain.Common;

namespace TeamTactics.Application.Users;

public class ProfileDto
{
    public string Username { get; set; }
    public string Email { get; set; }

    public ProfileDto(int id, string username, string email) { 
        Username = username;
        Email = email;
    }
}
