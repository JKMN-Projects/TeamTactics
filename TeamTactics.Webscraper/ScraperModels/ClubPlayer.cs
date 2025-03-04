using TeamTactics.Webscraper.Attributes;

namespace TeamTactics.Webscraper.ScraperModels;

[ScraperSource(
    "https://fbref.com/en/squads/{clubId}/{clubName}-stats",
    "//table[@id='stats_standard_9']/tbody/tr[not(contains(@class, 'spacer') or contains(@class, 'thead'))]")]
public class ClubPlayer
{
    [Selector(xpath: ".//th[@data-stat='player']/a")]
    public string Player { get; set; }

    [Selector(xpath: ".//td[@data-stat='nationality']/a/span/span")]
    public string Nationality { get; set; }

    [Selector(xpath: ".//td[@data-stat='position']")]
    public string Postition { get; set; }

    [Selector(xpath: ".//td[@data-stat='age']")]
    public string Birthdate
    {
        get { return birthDate.ToString("yyyy-MM-dd"); } // Format as needed
        set
        {
            if (string.IsNullOrEmpty(value))
                return;

            var parts = value.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[0], out int age) && int.TryParse(parts[1], out int daysSinceBirthday))
            {
                birthDate = CalculateBirthdate(age, daysSinceBirthday);
            }
            else
            {
                if (DateTime.TryParse(value, out DateTime result))
                    birthDate = result;
            }
        }
    }
    private DateTime birthDate; // Changed from string to DateTime
    public static DateTime CalculateBirthdate(int age, int daysSinceBirthday)
    {
        DateTime currentDate = DateTime.Now;

        DateTime lastBirthday = currentDate.AddDays(-daysSinceBirthday);

        int birthYear = lastBirthday.Year - age;

        DateTime birthdate = new DateTime(birthYear, lastBirthday.Month, lastBirthday.Day);

        return birthdate;
    }
}
