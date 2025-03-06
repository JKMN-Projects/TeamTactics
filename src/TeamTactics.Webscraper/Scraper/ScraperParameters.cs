namespace TeamTactics.Webscraper.Scraper;

internal class ScraperParameters
{
    private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();

    public ScraperParameters WithParameter(string key, string value)
    {
        _parameters[key] = value;
        return this;
    }

    public string ApplyToTemplate(string template)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        string result = template;
        foreach (var param in _parameters)
        {
            result = result.Replace($"{{{param.Key}}}", param.Value);
        }
        return result;
    }
}

