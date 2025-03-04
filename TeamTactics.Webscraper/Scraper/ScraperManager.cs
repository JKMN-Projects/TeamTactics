using TeamTactics.Webscraper.ScraperModels;

namespace TeamTactics.Webscraper.Scraper;

public class ScraperManager
{
    private readonly HttpClient _httpClient;
    public ScraperManager()
    {
        _httpClient = new HttpClient();
    }
    public async Task<List<Club>> GetClubs()
    {
        var scraper = new WebScraper(_httpClient);
        var clubs = await scraper.ScrapeListAsync<Club>();
        return clubs;
    }
    public async Task<List<ClubPlayer>> GetSquadPlayers(string clubId, string clubName)
    {
        var parameters = new ScraperParameters()
            .WithParameter("clubId", clubId)
            .WithParameter("teamId", clubName);
        var scraper = new WebScraper(_httpClient, parameters);
        var squadPlayers = await scraper.ScrapeListAsync<ClubPlayer>();
        return squadPlayers;
    }
    public async Task<List<PremierLeagueFixture>> GetFixtures()
    {
        var scraper = new WebScraper(_httpClient);
        var fixtures = await scraper.ScrapeListAsync<PremierLeagueFixture>();
        return fixtures;
    }
    public async Task<List<PlayerStats>> GetPlayerStats(string matchId, string matchName, string teamId)
    {
        var parameters = new ScraperParameters()
            .WithParameter("matchId", matchId)
            .WithParameter("matchName", matchName)
            .WithParameter("teamId", teamId);

        var scraper = new WebScraper(_httpClient, parameters);
        var playerStats = await scraper.ScrapeListAsync<PlayerStats>();
        return playerStats;
    }

}
