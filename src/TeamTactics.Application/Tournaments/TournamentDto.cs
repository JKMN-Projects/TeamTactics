namespace TeamTactics.Application.Tournaments;

public sealed record TournamentDto (int id, string name, string description, string inviteCode, string competitionName, string ownerUsername, int ownerUserId);
