
namespace TeamTactics.Domain.Teams
{
    public class TeamPlayer
    {
        public int PlayerId { get; private set; }
        public int ClubId { get; private set; }
        public bool IsCaptain { get; private set; }

        public TeamPlayer(int playerId, int clubId)
        {
            PlayerId = playerId;
            ClubId = clubId;
            IsCaptain = false;
        }

        internal void SetCaptain()
        {
            IsCaptain = true;
        }

        internal void UnsetCaptain()
        {
            IsCaptain = false;
        }
    }
}
