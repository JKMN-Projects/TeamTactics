using TeamTactics.Domain.Common;

namespace TeamTactics.Application.Matches;

public class Match : Entity
{
    public string HomeClubName { get; private set; }
    public string AwayClubName { get; private set; }
    public int HomeClubScore { get; private set; }
    public int AwayClubScore { get; private set; }
    public string CompetitionName { get; private set; }
    public DateTime UtcTimestamp { get; private set; }

    public Match(string homeClubName, string awayClubName, int homeClubScore, int awayClubScore, string competitionName, DateTime utcTimestamp)
    {
        HomeClubName = homeClubName;
        AwayClubName = awayClubName;
        HomeClubScore = homeClubScore;
        AwayClubScore = awayClubScore;
        CompetitionName = competitionName;
        UtcTimestamp = utcTimestamp;
    }
}
