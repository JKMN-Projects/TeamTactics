using TeamTactics.Webscraper.Attributes;

namespace TeamTactics.Webscraper.ScraperModels;


[ScraperSource(
"https://fbref.com/en/comps/9/schedule/Premier-League-Scores-and-Fixtures",
"//table[@id='sched_2024-2025_9_1']/tbody/tr[not(contains(@class, 'spacer') or contains(@class, 'thead'))]")]
public class Fixture
{
    private string id;
    private string homeTeamId;
    private string awayTeamId;
    private string matchName;
    private int homeScore;
    private int awayScore;

    [Selector(xpath: ".//td[@data-stat='match_report']/a", attribute: "href")]
    public string Id
    {
        get { return id; }
        set
        {

            id = value.Split("/")[3];
        }
    }

    [Selector(xpath: ".//td[@data-stat='date']/a")]
    public string Date { get; set; }

    [Selector(xpath: ".//td[@data-stat='home_team']/a")]
    public string HomeTeam { get; set; }
    [Selector(xpath: ".//td[@data-stat='home_team']/a", attribute: "href")]
    public string HomeTeamId
    {
        get { return homeTeamId; }
        set
        {

            homeTeamId = value.Split("/")[3];
        }
    }

    [Selector(xpath: ".//td[@data-stat='away_team']/a")]
    public string AwayTeam { get; set; }
    [Selector(xpath: ".//td[@data-stat='home_team']/a", attribute: "href")]
    public string AwayTeamId
    {
        get { return awayTeamId; }
        set
        {

            awayTeamId = value.Split("/")[3];
        }
    }
    [Selector(xpath: ".//td[@data-stat='score']/a")]
    internal string Score
    {
        set
        {
            homeScore = int.Parse(value.Split("-")[0]);
            awayScore = int.Parse(value.Split("-")[1]);
        }
    }

    public int HomeScore 
    {
        get { return homeScore; }
    }
    public int AwayScore 
    {
        get { return awayScore; }
    }

    [Selector(xpath: ".//td[@data-stat='match_report']/a", attribute: "href")]
    public string MatchReportUrl
    {
        get { return matchName; }
        set
        {

            matchName = value.Split("/")[4];
        }
    }

}


