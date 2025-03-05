﻿
namespace TeamTactics.Domain.Competitions;

public class Competition : Entity
{
    public string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public Competition(int id, string name, DateOnly startDate, DateOnly endDate) : base(id)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }
}
