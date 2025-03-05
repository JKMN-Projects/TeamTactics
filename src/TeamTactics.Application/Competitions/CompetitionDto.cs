namespace TeamTactics.Application.Competitions;

public sealed partial class CompetitionManager
{
    public sealed record CompetitionDto(
        int Id,
        string Name,
        DateOnly StartDate,
        DateOnly EndDate);
}