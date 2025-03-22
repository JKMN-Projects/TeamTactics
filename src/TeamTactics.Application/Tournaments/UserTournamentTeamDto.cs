
namespace TeamTactics.Application.Tournaments
{
    public sealed record UserTournamentTeamDto(
        int TeamId,
        string TeamName,
        int TournamentId,
        string TournamentName,
        string CompetitionName,
        int TotalPoints,
        DateOnly StartDate,
        DateOnly EndDate);
}
