namespace TeamTactics.Application.Matches
{
    public sealed record MatchDto(
        string HomeTeamName,
        string AwayTeamName,
        int HomeTeamScore,
        int AwayTeamScore,
        string CompetitionName,
        DateTime MatchDate
    );
}
