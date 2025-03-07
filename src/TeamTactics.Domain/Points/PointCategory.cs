
namespace TeamTactics.Domain.Points;

public class PointCategory : Entity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public double PointAmount { get; set; }
    public bool Active { get; set; }

    public PointCategory(int id, string name, string description, double pointAmount, bool active) : base(id)
    {
        Name = name;
        Description = description;
        PointAmount = pointAmount;
        Active = active;
    }
}
