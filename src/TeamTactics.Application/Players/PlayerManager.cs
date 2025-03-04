using TeamTactics.Application.Users;
using TeamTactics.Domain.Players;
using TeamTactics.Webscraper.Scraper;

namespace TeamTactics.Application.Players;

public sealed class PlayerManager
{
    private readonly IPlayerRepository _playerRepository;
    public PlayerManager(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
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
        var clubs = await scraperManager.GetClubs();

        await _playerRepository.InsertClubsBulk(clubs);
    }
    public async Task StartPlayerPopulation()
    {
        var clubs = await _playerRepository.GetClubs();
        
        var scraperManager = new ScraperManager();
        var players = new List<Player>();
        foreach (var club in clubs)
        {
            var squadPlayers = await scraperManager.GetSquadPlayers(club.ExternalId, club.Name);
            foreach (var squadPlayer in squadPlayers)
            {
                var names = squadPlayer.Name.Split(' ').ToList();
                var player = new Player(firstName: names.first, lastName: String.Join(names.ToArray()), birthdate: squadPlayer.Birthday, "", 0);
                players.Add(player);
            }
        }
        //Insert players into the database
        await _playerRepository.InsertPlayersBulk(players);
    }

}
