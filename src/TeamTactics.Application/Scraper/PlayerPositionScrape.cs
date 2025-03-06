namespace TeamTactics.Application.Scraper;

public class PlayerPositionScrape(int id, string name)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
}
