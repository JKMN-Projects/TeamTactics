using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Application.Points;

public sealed record PointResultDto(string playerName, string clubName, string pointCategoryName, int occurrences, int totalPoints);
