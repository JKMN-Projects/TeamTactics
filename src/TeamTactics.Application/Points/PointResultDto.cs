

namespace TeamTactics.Application.Points;

public sealed record PointResultDto(
    string PlayerName,
    string ClubName,
    string PointCategoryName,
    int Occurrences,
    decimal TotalPoints);
