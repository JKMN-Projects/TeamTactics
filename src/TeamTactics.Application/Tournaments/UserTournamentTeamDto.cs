
namespace TeamTactics.Application.Tournaments
{
    public sealed record UserTournamentTeamDto(
        int TeamId,
        string TeamName,
        int TournamentId,
        string TournamentName,
        string CompetitionName,
        DateOnly StartDate,
        DateOnly EndDate)
    {
        public decimal TotalPoints { get; set; }
    };
}
