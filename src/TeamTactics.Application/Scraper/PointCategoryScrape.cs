namespace TeamTactics.Application.Scraper;

public class PointCategoryScrape
{
    public PointCategoryScrape(int id, string name, decimal point_amount, bool active, string external_Id)
    {
        Id = id;
        Name = name;
        PointAmount = point_amount;
        Active = active;
        ExternalId = external_Id;

    }
    public int Id { get; private set; }
    public string Name { get; private set; }
    public decimal PointAmount { get; private set; }
    public bool Active { get; private set; }
    public string ExternalId { get; private set; }
}
