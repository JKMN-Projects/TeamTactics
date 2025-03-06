using TeamTactics.Application.Scraper;
using TeamTactics.Application.Users;
using TeamTactics.Domain.Players;
using TeamTactics.Webscraper.Scraper;

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

    public async Task StartClubPopulation()
    {
        var scraperManager = new ScraperManager();
        IEnumerable<Webscraper.ScraperModels.Club> clubs = await scraperManager.GetClubs();
        var clubScrapes = clubs.Select(c => new ClubScrape(c.Name, (c.Link = c.Link.Replace("https://fbref.com/en/squads/", "").Replace("-stats", "").Split("/")[0])));
        await _scraperRepository.InsertClubsAndClubCompetitionsBulk(clubScrapes, "9");
    }
    public async Task StartPlayerPopulation()
    {
        var scraperManager = new ScraperManager();
        var clubs = await _scraperRepository.GetClubs();
        var positions = await _scraperRepository.GetPlayerPositions();
        int count = 0;
        foreach (var club in clubs)
        {
            if(count >= 7)
            {
                await Task.Delay(60000);
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
                var player = new PlayerScrape(
                    firstName: names.First(),
                    lastName: String.Join(" ", names.Skip(1)),
                    birthdate: DateOnly.Parse(clubPlayer.Birthdate),
                    externalId: clubPlayer.Id,
                    positionId: positions.Where(c => c.Name == convertedPos).Select(c => c.Id).First()
                    
                );
                players.Add(player);
            }
            var insertedPlayers = await _scraperRepository.InsertPlayersBulk(players);
            await _scraperRepository.InsertPlayerContractsBulk(insertedPlayers.Select(p => new PlayerContractScrape(active: true, club.Id, p.Id)));
        }
    }

}
