namespace TeamTactics.Application.Scraper;

public class PlayerScrape(string firstName, string lastName, DateOnly birthdate, string externalId, int positionId)
{
    public int Id { get; set; }
    public string FirstName { get; private set; } = firstName;
    public string LastName { get; private set; } = lastName;
    public DateOnly BirthDate { get; private set; } = birthdate;
    public string ExternalId { get; private set; } = externalId;
    public int PositionId { get; private set; } = positionId;
}
