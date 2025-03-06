
namespace TeamTactics.Domain.Tournaments;

public class Tournament : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int CreatedByUserId { get; private set; }
    public int CompetitionId { get; private set; }

    public string InviteCode { get; private set; }

    public Tournament(string name, int createdByUserId, int competitionId, string description = "", string inviteCode = "")
    {
        Name = name;
        Description = description;
        CreatedByUserId = createdByUserId;
        CompetitionId = competitionId;
        InviteCode = inviteCode;
    }

    public void SetInviteCode(string newCode) => this.InviteCode = newCode;
}
