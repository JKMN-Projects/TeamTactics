namespace TeamTactics.Application.Scraper;

public class PointCategoryScrape(int id, string name, double pointAmount, bool active)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public double PointAmount { get; set; } = pointAmount;
    public bool Active { get; set; } = active;
}
