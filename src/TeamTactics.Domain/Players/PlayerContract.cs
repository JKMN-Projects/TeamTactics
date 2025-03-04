using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Domain.Players;
public class PlayerContract
{
    public PlayerContract(bool active, int clubId, int playerId)
    {
        Active = active;
        ClubId = clubId;
        PlayerId = playerId;
    }

    public int Id { get; set; }
    public bool Active { get; set; }
    public int ClubId { get; set; }
    public int PlayerId { get; set; }
}
