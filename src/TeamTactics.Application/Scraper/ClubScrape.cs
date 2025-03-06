namespace TeamTactics.Application.Scraper;

public class ClubScrape(string name, string external_id)
{
    public int Id { get; set; }
    public string Name { get; set; } = name;
    public string ExternalId { get; set; } = external_id;
}
