
namespace TeamTactics.Domain.Tournaments;

public class Tournament : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int CreatedByUserId { get; private set; }
    public int CompetitionId { get; private set; }

    public string InviteCode { get; private set; }

    #region Default Constructor
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Tournament() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    #endregion

    public Tournament(string name, int createdByUserId, int competitionId, string description = "")
    {
        Name = name;
        Description = description;
        CreatedByUserId = createdByUserId;
        CompetitionId = competitionId;
    }

    public void SetInviteCode(string newCode) => this.InviteCode = newCode;
}
