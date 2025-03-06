using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Domain.Tournaments
{
    public sealed record TournamentTeamsDto(int TeamId, string TeamName, int UserId, string Username);
}
