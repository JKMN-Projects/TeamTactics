using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Domain.Points;

public class PointCategory(int id, string name, double pointAmount, bool active)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public double PointAmount { get; set; } = pointAmount;
    public bool Active { get; set; } = active;
}
