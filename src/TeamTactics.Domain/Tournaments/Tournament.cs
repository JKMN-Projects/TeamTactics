
namespace TeamTactics.Domain.Tournaments;

public class Tournament
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public int CompetitionId { get; private set; }
    public int CreatedByUserId { get; private set; }

    public Tournament(string name, int competitionId, int createdByUserId)
    {
        Name = name;
        CompetitionId = competitionId;
        CreatedByUserId = createdByUserId;
    }
}
