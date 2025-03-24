

namespace TeamTactics.Application.Teams;

public class TeamPlayerDto
{
    public int Id {  get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool Captain { get; set; }
    public int ClubId { get; set; }
    public string ClubName { get; set; }
    public string ClubShorthand { get; set; }
    public int PositionId { get; set; }
    public string PositionName { get; set; }
}
