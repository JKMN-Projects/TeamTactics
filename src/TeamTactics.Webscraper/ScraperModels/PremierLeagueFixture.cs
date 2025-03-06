using TeamTactics.Webscraper.Attributes;

namespace TeamTactics.Webscraper.ScraperModels;


[ScraperSource(
"https://fbref.com/en/comps/9/schedule/Premier-League-Scores-and-Fixtures",
"//table[@id='sched_2024-2025_9_1']/tbody/tr[not(contains(@class, 'spacer') or contains(@class, 'thead'))]")]
public class PremierLeagueFixture
{
    [Selector(xpath: ".//td[@data-stat='date']/a")]
    public string Date { get; set; }

    [Selector(xpath: ".//td[@data-stat='home_team']/a")]
    public string HomeTeam { get; set; }

    [Selector(xpath: ".//td[@data-stat='away_team']/a")]
    public string AwayTeam { get; set; }

    [Selector(xpath: ".//td[@data-stat='match_report']/a", attribute: "href", transform: "https://fbref.com{0}")]
    public string MatchReportUrl { get; set; }

    public List<PlayerStats> HomeTeamPlayerStats { get; private set; }
    public List<PlayerStats> AwayTeamPlayerStats { get; private set; }

    public void SetTeamPlayerStats(List<PlayerStats> playerstats, bool homeTeam)
    {
        if (homeTeam)
        {
            HomeTeamPlayerStats = playerstats;
        }
        else
        {
            AwayTeamPlayerStats = playerstats;
        }
    }
}


