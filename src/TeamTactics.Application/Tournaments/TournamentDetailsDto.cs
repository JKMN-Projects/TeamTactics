namespace TeamTactics.Application.Tournaments
{
    public sealed record TournamentDetailsDto(
        int Id,
        string Name,
        string Description,
        string InviteCode,
        string CompetitionName,
        int OwnerUserId,
        string OwnerUsername);
}

