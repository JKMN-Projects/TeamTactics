
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Application.Tournaments
{
    public class TournamentTeamsDto
    {
        public int TeamId { get; private set; }
        public string TeamName { get; private set; }
        public decimal TotalPoints { get; private set; }

        public TournamentTeamsDto(int teamId, string teamName)
        {
            TeamId = teamId;
            TeamName = teamName;
        }

        public void UpdateTotalPoints(decimal newTotal) => TotalPoints = newTotal;
    }
}
