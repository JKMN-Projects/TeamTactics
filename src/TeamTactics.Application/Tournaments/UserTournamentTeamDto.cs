
namespace TeamTactics.Application.Tournaments
{
    public sealed record UserTournamentTeamDto(
        string TeamName,
        int TournamentId,
        string TournamentName,
        string CompetitionName,
        int TotalPoints,
        DateTime StartDate,
        DateTime EndDate);
}
