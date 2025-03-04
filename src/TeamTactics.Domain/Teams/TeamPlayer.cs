
namespace TeamTactics.Domain.Teams
{
    public class TeamPlayer
    {
        public int PlayerId { get; private set; }
        public bool IsCaptain { get; private set; }

        public TeamPlayer(int playerId, bool isCaptain)
        {
            PlayerId = playerId;
            IsCaptain = isCaptain;
        }
    }
}
