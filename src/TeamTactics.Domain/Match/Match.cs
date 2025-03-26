
namespace TeamTactics.Domain.Matches;

public class Match : Entity
{
    public int HomeClubId { get; private set; }
    public int AwayClubId { get; private set; }
    public int HomeClubScore { get; private set; }
    public int AwayClubScore { get; private set; }
    public int CompetitionId { get; private set; }
    public DateTime Timestamp { get; private set; }

    public Match(int homeClubId, int awayClubId, int homeClubScore, int awayClubScore, int competitionId, DateTime utcTimestamp)
    {
        HomeClubId = homeClubId;
        AwayClubId = awayClubId;
        HomeClubScore = homeClubScore;
        AwayClubScore = awayClubScore;
        CompetitionId = competitionId;
        Timestamp = utcTimestamp;
    }
}
