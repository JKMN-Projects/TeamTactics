namespace TeamTactics.Application.Bulletins
{
    public sealed record BulletinDto(
            int Id,
            string Text,
            DateTime CreatedTime,
            DateTime? LastEditedTime,
            int UserId,
            string Username);
}
