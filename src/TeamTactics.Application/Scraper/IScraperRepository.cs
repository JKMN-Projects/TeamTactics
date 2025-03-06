using System.Data;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Points;

namespace TeamTactics.Application.Scraper;

public interface IScraperRepository
{
    public Task<IEnumerable<PlayerPositionScrape>> GetPlayerPositions();

    public Task<IEnumerable<PointCategoryScrape>> GetPointCategories();
    public Task<IEnumerable<ClubScrape>> GetClubs();

    public Task<IEnumerable<ClubScrape>> InsertClubsAndClubCompetitionsBulk(IEnumerable<ClubScrape> clubs, string competitionExternalId);

    public Task<IEnumerable<PlayerScrape>> InsertPlayersBulk(IEnumerable<PlayerScrape> players);

    public Task<IEnumerable<PlayerContractScrape>> InsertPlayerContractsBulk(IEnumerable<PlayerContractScrape> contracts);
    public Task<IEnumerable<MatchResultScrape>> InsertMatchResultsBulk(IEnumerable<MatchResultScrape> matchResults);

    public Task<IEnumerable<MatchPlayerPointScrape>> InsertMatchPlayerPointsBulk(IEnumerable<MatchPlayerPointScrape> matchPlayerPoints);
}
