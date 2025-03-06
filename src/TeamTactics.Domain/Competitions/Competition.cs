
namespace TeamTactics.Domain.Competitions;

public class Competition : Entity
{
    public string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    #region Default Constructor
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Competition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    #endregion

    public Competition(string name, DateOnly startDate, DateOnly endDate) : base(default)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }
}
