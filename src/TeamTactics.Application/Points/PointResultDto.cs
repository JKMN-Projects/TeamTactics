

namespace TeamTactics.Application.Points;

public sealed record PointResultDto(
    string ClubName,
    string PlayerName,
    string PointCategoryName,
    int Occurrences,
    decimal PointAmount,
    decimal TotalPoints);
