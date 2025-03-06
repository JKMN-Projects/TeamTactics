using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Domain.Tournaments
{
    public class TournamentTeamsDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
    }
}
