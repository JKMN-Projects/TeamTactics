using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamTactics.Domain.Teams;

namespace TeamTactics.Application.Teams
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; private set; }
        public TeamStatus Status { get; private set; }
        public bool IsLocked { get { return this.Status == TeamStatus.Locked; } }
        public string Formation { get; private set; }
        public int UserId { get; private set; }
        public int TournamentId { get; private set; }
        public List<TeamPlayerDto> Players { get; private set; }

        //database constructor
        public TeamDto(int id, string name, TeamStatus status, string formation, int userId, int tournamentId, List<TeamPlayerDto> players)
        {
            Id = id;
            Name = name;
            Status = status;
            Formation = formation;
            UserId = userId;
            TournamentId = tournamentId;
            Players = players;
        }
    }
}
