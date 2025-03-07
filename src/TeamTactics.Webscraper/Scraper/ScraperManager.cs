using TeamTactics.Webscraper.ScraperModels;

namespace TeamTactics.Webscraper.Scraper;

public class ScraperManager
{
    private readonly HttpClient _httpClient;
    public ScraperManager()
    {
        _httpClient = new HttpClient();
    }
    /// <summary>
    /// Method used to get all clubs
    /// Hardcoded to premierleague right now
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="HttpProtocolException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="UriFormatException"></exception>
    public async Task<List<Club>> GetClubs()
    {
        var scraper = new WebScraper(_httpClient);
        var clubs = await scraper.ScrapeListAsync<Club>();
        return clubs;
    }
    /// <summary>
    /// Method used to get squad players on a club
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="clubName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="HttpProtocolException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="UriFormatException"></exception>
    public async Task<List<ClubPlayer>> GetSquadPlayers(string clubId, string clubName)
    {

        var parameters = new ScraperParameters()
            .WithParameter("clubId", clubId)
            .WithParameter("clubName", clubName);
        var scraper = new WebScraper(_httpClient, parameters);
        var squadPlayers = await scraper.ScrapeListAsync<ClubPlayer>();
        return squadPlayers;
    }
    /// <summary>
    /// Method used to scrape all fixtures
    /// Hardcoded right now to premierleague
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="HttpProtocolException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="UriFormatException"></exception>
    public async Task<List<Fixture>> GetFixtures()
    {
        var scraper = new WebScraper(_httpClient);
        var fixtures = await scraper.ScrapeListAsync<Fixture>();
        return fixtures;
    }
    /// <summary>
    /// Method to get playerstats on a match for a team
    /// </summary>
    /// <param name="matchId"></param>
    /// <param name="matchName"></param>
    /// <param name="teamId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="HttpProtocolException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="UriFormatException"></exception> 
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
