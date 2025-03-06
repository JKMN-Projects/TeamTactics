namespace TeamTactics.Application.Teams;

public class TeamPointsDto
{

    public decimal TotalPoints { get; set; }

    public TeamPointsDto(decimal totalPoints)
    {
        TotalPoints = totalPoints;
    }
}
