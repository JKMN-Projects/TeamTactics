
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Teams.Exceptions;

namespace TeamTactics.Domain.Teams
{
    public class Team
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public TeamStatus Status { get; private set; }
        public int UserId { get; private set; }
        public int CompetitionId { get; private set; }

        private readonly List<TeamPlayer> _players = new List<TeamPlayer>();
        public IReadOnlyCollection<TeamPlayer> Players => _players.AsReadOnly();

        private readonly List<TournamentEnrollments> _enrollments = new List<TournamentEnrollments>();
        public IReadOnlyCollection<TournamentEnrollments> Enrollments => _enrollments.AsReadOnly();

        public Team(string name, int userId, int competitionId)
        {
            Name = name;
            UserId = userId;
            CompetitionId = competitionId;
            Status = TeamStatus.Draft;
        }

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

            _players.Add(new TeamPlayer(player.Id, isCaptain: false));
        }

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
                int currentCaptainIndex = _players.IndexOf(currentCaptain);
                _players[currentCaptainIndex] = new TeamPlayer(currentCaptain.PlayerId, isCaptain: false);
            }

            int index = _players.IndexOf(player);
            _players[index] = new TeamPlayer(player.PlayerId, isCaptain: true);
        }

        public void Lock()
        {
            Status = TeamStatus.Locked;
        }

        public void EnrollInTournament(int tournamentId)
        {
            if (Status == TeamStatus.Locked)
            {
                throw new TeamLockedException();
            }

            _enrollments.Add(new TournamentEnrollments(tournamentId));
        }
    }
}
