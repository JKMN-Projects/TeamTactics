namespace TeamTactics.Domain.Players;

public class Player
{
    public int Id { get; set; }
    public string Name { get; private set; }
    public string ExternalId { get; private set; }
    public int ClubId { get; private set; }
    public int PositionId { get; private set; }

    public Player(string name, string externalId)
    {
        
    }
}
