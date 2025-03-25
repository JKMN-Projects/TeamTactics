namespace TeamTactics.Application.Tournaments
{
    public sealed record TournamentDetailsDto(
        int Id,
        string Name,
        string Description,
        string InviteCode,
        int CompetitionId,
        string CompetitionName,
        int OwnerUserId,
        string OwnerUsername);
}

