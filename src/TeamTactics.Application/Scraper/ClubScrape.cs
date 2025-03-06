namespace TeamTactics.Application.Scraper;

public class ClubScrape(string name, string external_id, int id = 0)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string ExternalId { get; set; } = external_id;
}
