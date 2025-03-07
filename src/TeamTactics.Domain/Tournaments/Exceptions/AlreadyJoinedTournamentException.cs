namespace TeamTactics.Domain.Tournaments.Exceptions;

public sealed class AlreadyJoinedTournamentException : DomainException
{
    public AlreadyJoinedTournamentException(int userId, int tournamentId) 
        : base("Tournament.AlreadyJoined", $"The user '{userId}' has already joined the tournament '{tournamentId}'.")
    {
    }
}
