namespace TeamTactics.Domain.Players;

public class Player
{
    public int Id { get; set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public string ExternalId { get; private set; }
    public int ClubId { get; private set; }
    public int PositionId { get; private set; }

    public Player(string firstName, string lastName, DateOnly birthdate, string externalId, int clubId, int positionId)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthdate;
        ExternalId = externalId;
        ClubId = clubId;
        PositionId = positionId;
    }
}
