using TeamTactics.Webscraper.Attributes;

namespace TeamTactics.Webscraper.ScraperModels;

[ScraperSource(
    urlTemplate: "https://fbref.com/en/matches/{matchId}/{matchName}",
    rowsXPathTemplate: "//table[@id='stats_{teamId}_summary']/tbody/tr[not(contains(@class, 'spacer') or contains(@class, 'thead'))]")]
public class PlayerStats
{
    [Selector(xpath: ".//th[@data-stat='player']/a")]
    public string Player { get; set; }

    [Selector(xpath: ".//td[@data-stat='minutes']")]
    public int Minutes { get; set; }

    [Selector(xpath: ".//td[@data-stat='assists']")]
    public int Assists { get; set; }

    [Selector(xpath: ".//td[@data-stat='shots']")]
    public int Shots { get; set; }

    [Selector(xpath: ".//td[@data-stat='blocks']")]
    public int Blocks { get; set; }

}

