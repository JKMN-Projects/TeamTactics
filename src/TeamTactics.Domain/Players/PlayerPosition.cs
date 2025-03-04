namespace TeamTactics.Domain.Players;

public class PlayerPosition
{
    public int Id { get; set; }
    public string Name { get; set; }

    public PlayerPosition(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
