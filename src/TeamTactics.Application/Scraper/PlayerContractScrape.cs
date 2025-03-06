namespace TeamTactics.Application.Scraper;

public class PlayerContractScrape(bool active, int clubId, int playerId)
{
    public int Id { get; set; }
    public bool Active { get; set; } = active;
    public int ClubId { get; set; } = clubId;
    public int PlayerId { get; set; } = playerId;
}
