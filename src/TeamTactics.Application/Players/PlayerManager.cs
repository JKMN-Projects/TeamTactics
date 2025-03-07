using System.Linq;
using TeamTactics.Application.Scraper;
using TeamTactics.Application.Users;
using TeamTactics.Domain.Players;
using TeamTactics.Webscraper.Scraper;
using TeamTactics.Webscraper.ScraperModels;

namespace TeamTactics.Application.Players;

public sealed class PlayerManager
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IScraperRepository _scraperRepository;
    private readonly Dictionary<string, string> _positions = new(){ { "FW", "Forward" },{"MF","Midfield" },{ "DF","Defender"},{ "GK","Goalkeeper"} };
    public PlayerManager(IPlayerRepository playerRepository, IScraperRepository scraperRepository)
    {
        _playerRepository = playerRepository;
        _scraperRepository = scraperRepository;
    }

    //Queries
    //Queries use dto models
    public async Task<IEnumerable<PlayerDto>> GetPlayersAsync(int? competitionId = null)
    {
        return await _playerRepository.GetPlayersAsync(competitionId);
    }

    //Commands
    //Commands use domain models

    public async Task StartClubPopulation(string externalCompetitionId)
    {
        var scraperManager = new ScraperManager();
        IEnumerable<Webscraper.ScraperModels.Club> clubs = await scraperManager.GetClubs();
        var clubScrapes = clubs.Select(c => new ClubScrape(c.Name, (c.Link = c.Link.Replace("https://fbref.com/en/squads/", "").Replace("-stats", "").Split("/")[0])));
        await _scraperRepository.InsertClubsAndClubCompetitionsBulk(clubScrapes, externalCompetitionId);
    }
    public async Task LoadFixtures(int competitionId, DateTime loadToDate)
    {

        var clubs = await _scraperRepository.GetClubs(competitionId);

        var pointCategories = await _scraperRepository.GetPointCategories();


        var latestMatch = await _scraperRepository.GetLatestMatch(competitionId);


        var scraperManager = new ScraperManager();
        var fixtures = await scraperManager.GetFixtures();
        if (latestMatch == null)
        {
            fixtures = fixtures.Where(f => DateTime.Parse(f.Date) > DateTime.MinValue && DateTime.Parse(f.Date) <= loadToDate).ToList();
        }
        else
        {
            fixtures = fixtures.Where(f => DateTime.Parse(f.Date) > latestMatch.Timestamp && DateTime.Parse(f.Date) <= loadToDate).ToList();
        }
        IEnumerable<MatchResultScrape> matchResults = fixtures.Select(f => new MatchResultScrape()
        {
            HomeClubId = clubs.Where(c => c.ExternalId == f.HomeTeamId).Select(c => c.Id).FirstOrDefault(),
            HomeClubScore = f.HomeScore, 
            AwayClubScore = f.AwayScore,
            AwayClubId = clubs.Where(c => c.ExternalId == f.AwayTeamId).Select(c => c.Id).FirstOrDefault(),
            ExternalId = f.Id,
            CompetitionId = competitionId,
            Timestamp = DateTime.Parse(f.Date),
            UrlName = f.MatchReportUrl,
        });
        if(fixtures.Count() > 0)
        {
            var insertedMatches = await _scraperRepository.InsertMatchResultsBulk(matchResults);
        }

    }
    public async Task LoadPlayerStatsForFixtures()
    {
        var fixtures = await _scraperRepository.GetFixturesWithNoStats();
        var pointCategories = await _scraperRepository.GetPointCategories();
        
        var scraperManager = new ScraperManager();
        int count = 0;
        foreach (var fixture in fixtures)
        {
            if (count >= 4)
            {
                await Task.Delay(60000);
                count = 0;
            }
            var homeClub = await _scraperRepository.GetClubById(fixture.HomeClubId);
            var homePlayerStats = await scraperManager.GetPlayerStats(fixture.ExternalId, fixture.UrlName, homeClub.ExternalId);
            var awayClub = await _scraperRepository.GetClubById(fixture.AwayClubId);
            var awayPlayerStats = await scraperManager.GetPlayerStats(fixture.ExternalId, fixture.UrlName, awayClub.ExternalId);

            IEnumerable<MatchPlayerPointScrape> playerPointScrapes = new List<MatchPlayerPointScrape>();
            playerPointScrapes = playerPointScrapes.Concat(await GetMatchPlayerPointScrapes(homePlayerStats, pointCategories, fixture.Id, homeClub.Id));
            playerPointScrapes = playerPointScrapes.Concat(await GetMatchPlayerPointScrapes(awayPlayerStats, pointCategories, fixture.Id, awayClub.Id));
            if (playerPointScrapes.Count() > 0)
            {
                await _scraperRepository.InsertMatchPlayerPointsBulk(playerPointScrapes);
            }
        }

    }
    private async Task<List<MatchPlayerPointScrape>> GetMatchPlayerPointScrapes(List<PlayerStats> playerStats, IEnumerable<PointCategoryScrape> pointCategories, int matchId, int clubId)
    {
        var playerPointScrapes = new List<MatchPlayerPointScrape>();
        foreach (var playerStat in playerStats)
        {
            var player = await _scraperRepository.GetPlayerIdByExternalIdAndClubId(playerStat.Id, clubId);
            if(player == null)
            {
                continue;
            }
            var properties = playerStat.GetType().GetProperties();
            foreach (var pointCategory in pointCategories)
            {
                var property = playerStat.GetType().GetProperty(pointCategory.Name);
                if (property != null)
                {
                    playerPointScrapes.Add(new MatchPlayerPointScrape()
                    {
                        MatchId = matchId,
                        PlayerId = player.Id,
                        PointCategoryId = pointCategory.Id,
                        Occurrences = (int)property.GetValue(playerStat)
                    });
                }
            }
        }
        return playerPointScrapes;
    }
    public async Task StartPlayerPopulation(int competition)
    {
        var scraperManager = new ScraperManager();
        var clubs = await _scraperRepository.GetClubs(competition);
        var positions = await _scraperRepository.GetPlayerPositions();
        int count = 0;
        foreach (var club in clubs)
        {
            if(count >= 7)
            {
                await Task.Delay(60000);
                count = 0;
            }
            var players = new List<PlayerScrape>();
            var clubPlayers = await scraperManager.GetSquadPlayers(club.ExternalId, club.Name);
            count++;
            foreach (var clubPlayer in clubPlayers)
            {
                if(String.IsNullOrWhiteSpace(clubPlayer.Nationality) || String.IsNullOrWhiteSpace(clubPlayer.Postition) || String.IsNullOrWhiteSpace(clubPlayer.Birthdate))
                {
                    continue;
                }
                var names = clubPlayer.Player.Split(' ').ToList();
                var convertedPos = _positions[clubPlayer.Postition.Split(",")[0]];
                var player = new PlayerScrape()
                {
                    FirstName = names.First(),
                    LastName = String.Join(" ", names.Skip(1)),
                    ExternalId = clubPlayer.Id,
                    BirthDate = DateOnly.Parse(clubPlayer.Birthdate),
                    PositionId = positions.Where(c => c.Name == convertedPos).Select(c => c.Id).First()
                };
                players.Add(player);
            }
            var insertedPlayers = await _scraperRepository.InsertPlayersBulk(players);
            await _scraperRepository.InsertPlayerContractsBulk(insertedPlayers.Select(p => new PlayerContractScrape(active: true, club.Id, p.Id)));
        }
    }

}
