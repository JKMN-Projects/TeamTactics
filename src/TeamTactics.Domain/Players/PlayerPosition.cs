namespace TeamTactics.Domain.Players;

public class PlayerPosition : Entity
{
    public string Name { get; set; }

    public PlayerPosition(int id, string name) : base(id)
    {
        Name = name;
    }
}
