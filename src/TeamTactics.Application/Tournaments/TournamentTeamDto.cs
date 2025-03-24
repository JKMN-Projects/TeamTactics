
namespace TeamTactics.Application.Tournaments
{
    public sealed record TournamentTeamDto
    {
        public int TeamId { get; init; }
        public string TeamName { get; init; }
        public decimal TotalPoints { get; set; }
        public decimal UserId { get; init; }

        public TournamentTeamDto(int teamId, string teamName, int userId)
        {
            TeamId = teamId;
            TeamName = teamName;
            UserId = userId;
        }
    }
}
