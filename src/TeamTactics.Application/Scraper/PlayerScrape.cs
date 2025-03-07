namespace TeamTactics.Application.Scraper;

public class PlayerScrape
{
    public int Id { get; set; }
    public string FirstName { get;  set; }
    public string LastName { get;  set; }
    public DateOnly BirthDate { get;  set; }
    public string ExternalId { get;  set; }
    public int PositionId { get;  set; }
}
