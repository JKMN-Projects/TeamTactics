
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams.Exceptions;

namespace TeamTactics.Domain.Teams
{
    public class Team : Entity
    {
        private const int MAX_PLAYERS_PER_CLUB = 2;
        public string Name { get; private set; }
        public TeamStatus Status { get; private set; }
        public int UserId { get; private set; }
        public int TournamentId { get; private set; }

        private readonly List<TeamPlayer> _players = new List<TeamPlayer>();
        public IReadOnlyCollection<TeamPlayer> Players => _players.AsReadOnly();

        public Team(string name, int userId, int tournamentId)
        {
            Name = name;
            UserId = userId;
            TournamentId = tournamentId;
            Status = TeamStatus.Draft;
        }

        //database constructor
        public Team(int id, string name, TeamStatus status, int userId, int tournamentId, List<TeamPlayer> players) : base(id)
        {
            Name = name;
            Status = status;
            UserId = userId;
            TournamentId = tournamentId;
            _players = players;
        }

        /// <summary>
        /// Add a player to the team
        /// </summary>
        /// <param name="player">The player to be added</param>
        /// <exception cref="TeamLockedException"></exception>
        /// <exception cref="PlayerAlreadyInTeamException"></exception>
        /// <exception cref="TeamFullException"></exception>
        /// <exception cref="MaximumPlayersFromSameClubReachedException"></exception>
        public void AddPlayer(Player player)
        {
            if (Status == TeamStatus.Locked)
            {
                throw new TeamLockedException();
            }

            if (_players.Any(p => p.PlayerId == player.Id))
            {
                throw new PlayerAlreadyInTeamException(player);
            }

            if (_players.Count == 11)
            {
                throw new TeamFullException();
            }

            if (_players.Where(p => p.ClubId == player.ActivePlayerContract.ClubId).Count() >= MAX_PLAYERS_PER_CLUB)
            {
                throw new MaximumPlayersFromSameClubReachedException(player.ActivePlayerContract.ClubId);
            }

            _players.Add(new TeamPlayer(player.Id, player.ActivePlayerContract.ClubId));
        }

        /// <summary>
        /// Remove a player from the team
        /// </summary>
        /// <param name="playerId"></param>
        /// <exception cref="TeamLockedException"></exception>
        /// <exception cref="PlayerNotOnTeamException"></exception>
        public void RemovePlayer(int playerId)
        {
            if (Status == TeamStatus.Locked)
            {
                throw new TeamLockedException();
            }

            var player = _players.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
            {
                throw new PlayerNotOnTeamException(playerId);
            }

            _players.Remove(player);
        }

        /// <summary>
        /// Set a player as captain of the team
        /// </summary>
        /// <param name="playerId"></param>
        /// <exception cref="TeamLockedException"></exception>
        /// <exception cref="PlayerNotOnTeamException"></exception>
        /// <exception cref="PlayerAlreadyCaptainException"></exception>
        public void SetCaptain(int playerId)
        {
            if (Status == TeamStatus.Locked)
            {
                throw new TeamLockedException();
            }

            var player = _players.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
            {
                throw new PlayerNotOnTeamException(playerId);
            }

            if (player.IsCaptain)
            {
                throw new PlayerAlreadyCaptainException(playerId);
            }

            var currentCaptain = _players.FirstOrDefault(p => p.IsCaptain);
            if (currentCaptain != null)
            {
                currentCaptain.UnsetCaptain();
            }

            player.SetCaptain();
        }

        /// <summary>
        /// Lock the team and prevent any further mutations.
        /// </summary>
        /// <exception cref="TeamLockedException"></exception>
        /// <exception cref="TeamNotFullException"></exception>
        /// <exception cref="NoCaptainException"></exception>
        public void Lock()
        {
            if (Status == TeamStatus.Locked) {
                throw new TeamLockedException();
            }

            if (_players.Count < TeamNotFullException.REQUIRED_NUMBER_OF_PLAYERS)
            {
                throw new TeamNotFullException(_players.Count);
            }

            if (_players.Count(p => p.IsCaptain) != 1)
            {
                throw new NoCaptainException();
            }

            Status = TeamStatus.Locked;
        }
    }
}
