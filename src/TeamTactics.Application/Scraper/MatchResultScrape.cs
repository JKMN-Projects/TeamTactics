namespace TeamTactics.Application.Scraper;

public class MatchResultScrape
{
    public int Id { get; set; }
    public int HomeClubScore { get; set; }
    public int AwayClubScore { get; set; }
    public DateTime Timestamp { get; set; }
    public string UrlName { get; set; }
    public string ExternalId { get; set; }
    public int HomeClubId { get; set; }
    public int AwayClubId { get; set; }
    public int CompetitionId { get; set; }

}
