namespace TeamTactics.Domain.Bulletins;

public class Bulletin : Entity
{

    public string Text { get; private set; }
    public DateTime CreatedTime { get; private set; }
    public DateTime? LastEditedTime { get; private set; }
    public int TournamentId { get; private set; }
    public int UserId { get; private set; }

    public Bulletin(string text, DateTime createdTime, DateTime? lastEditedTime, int tournamentId, int userId)
    {
        Text = text;
        CreatedTime = createdTime;
        LastEditedTime = lastEditedTime;
        TournamentId = tournamentId;
        UserId = userId;
    }

    public Bulletin(int id, string text, DateTime createdTime, DateTime? lastEditedTime, int tournamentId, int userId) : this(text, createdTime, lastEditedTime, tournamentId, userId)
    {
        SetId(id); 
    }
}
