using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Domain.Points;

public class PointCategory : Entity
{
    public string Name { get; set; }
    public double PointAmount { get; set; }
    public bool Active { get; set; }

    public PointCategory(int id, string name, double pointAmount, bool active) : base(id)
    {
        Name = name;
        PointAmount = pointAmount;
        Active = active;
    }
}
