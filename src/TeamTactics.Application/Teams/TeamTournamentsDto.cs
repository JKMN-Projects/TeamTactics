namespace TeamTactics.Application.Teams;

public class TeamTournamentsDto
{
    public int TeamId { get; private set; }
    public string TeamName { get; private set; }
    public decimal TotalPoints { get; private set; }
    public string TournamentName { get; private set; }

    public TeamTournamentsDto(int teamId, string teamName, string tournamentName)
    {
        TeamId = teamId;
        TeamName = teamName;
        TournamentName = tournamentName;
    }

    public void UpdateTotalPoints(decimal newTotal) => TotalPoints = newTotal;
}
