
namespace TeamTactics.Domain.Players;

public class Player
{
    public int Id { get; set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public string ExternalId { get; private set; }
    public int PositionId { get; private set; }

    private readonly List<PlayerContract> _playerContracts = new List<PlayerContract>();
    public IReadOnlyCollection<PlayerContract> PlayerContracts => _playerContracts.AsReadOnly();
    public PlayerContract ActivePlayerContract => PlayerContracts.Single(pc => pc.Active);

    public Player(string firstName, string lastName, DateOnly birthdate, string externalId, int positionId)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthdate;
        ExternalId = externalId;
        PositionId = positionId;
    }
}
