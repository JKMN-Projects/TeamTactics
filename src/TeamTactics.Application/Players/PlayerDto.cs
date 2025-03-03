
namespace TeamTactics.Application.Players;

public record PlayerDto(
    int Id,
    string FirstName,
    string LastName,
    bool Captain,
    int ClubId,
    string ClubName,
    int PositionId,
    string PostionName);

