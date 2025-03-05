namespace TeamTactics.Domain.Players;

public class PlayerContract
{
    public int Id { get; private set; }
    public int ClubId { get; private set; }
    public bool Active { get; private set; }
    public int PlayerId { get; private set; }

    public PlayerContract(int clubId, int playerId)
    {
        ClubId = clubId;
        Active = true;
        PlayerId = playerId;
    }

    internal void DeactivateContract()
    {
        Active = false;
    }
}
