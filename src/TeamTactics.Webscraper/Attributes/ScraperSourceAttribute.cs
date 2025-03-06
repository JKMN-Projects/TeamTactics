namespace TeamTactics.Webscraper.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal class ScraperSourceAttribute : Attribute
{
    public string UrlTemplate { get; }
    public string RowsXPathTemplate { get; }

    public ScraperSourceAttribute(string urlTemplate, string rowsXPathTemplate)
    {
        UrlTemplate = urlTemplate;
        RowsXPathTemplate = rowsXPathTemplate;
    }
}

