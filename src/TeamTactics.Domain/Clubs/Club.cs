namespace TeamTactics.Domain.Clubs;

public class Club : Entity
{
    public string Name { get; set; }
    public string ExternalId { get; set; }

    public Club(string name, string externalId)
    {
        Name = name;
        ExternalId = externalId;
    }
}

