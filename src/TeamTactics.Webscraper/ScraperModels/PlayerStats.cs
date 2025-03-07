using TeamTactics.Webscraper.Attributes;

namespace TeamTactics.Webscraper.ScraperModels;

[ScraperSource(
    urlTemplate: "https://fbref.com/en/matches/{matchId}/{matchName}",
    rowsXPathTemplate: "//table[@id='stats_{teamId}_summary']/tbody/tr[not(contains(@class, 'spacer') or contains(@class, 'thead'))]")]
public class PlayerStats
{

    private string id;

    [Selector(xpath: ".//th[@data-stat='player']/a", attribute: "href")]
    public string Id
    {
        get { return id; }
        set
        {

            id = value.Split("/")[3];
        }
    }
    [Selector(xpath: ".//th[@data-stat='player']/a")]
    public string Player { get; set; }

    [Selector(xpath: ".//td[@data-stat='minutes']")]
    public int Minutes { get; set; }

    [Selector(xpath: ".//td[@data-stat='goals']")]
    public int Goal { get; set; }

    [Selector(xpath: ".//td[@data-stat='assists']")]
    public int Assist { get; set; }

    [Selector(xpath: ".//td[@data-stat='shots']")]
    public int Shot { get; set; }

    [Selector(xpath: ".//td[@data-stat='blocks']")]
    public int Block { get; set; }

}

