using TeamTactics.Webscraper.Attributes;

namespace TeamTactics.Webscraper.ScraperModels;

[ScraperSource(
    "https://fbref.com/en/comps/9/Premier-League-Stats",
    "//table[@id='results2024-202591_overall']/tbody/tr[not(contains(@class, 'spacer') or contains(@class, 'thead'))]")]
public class Club
{
    [Selector(xpath: ".//td[@data-stat='team']/a")]
    public string Name { get; set; }

    [Selector(xpath: ".//td[@data-stat='team']/a", attribute: "href", transform: "https://fbref.com{0}")]
    public string Link { get; set; }

    public List<ClubPlayer> SquadPlayers { get; private set; }

    public void SetSquadPlayers(List<ClubPlayer> players)
    {
        SquadPlayers = players;
    }
}

