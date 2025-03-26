
namespace TeamTactics.Domain.Matches;

public class MatchPlayerPoint : Entity
{
    public int PlayerId { get; private set; }
    public int MatchId { get; private set; }
    public int PointCategoryId { get; private set; }
    public int Occurrences { get; private set; }
    public MatchPlayerPoint(int playerId, int matchId, int pointCategoryId, int occurrences)
    {
        PlayerId = playerId;
        MatchId = matchId;
        PointCategoryId = pointCategoryId;
        Occurrences = occurrences;
    }
}
