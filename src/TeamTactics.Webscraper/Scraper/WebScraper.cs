using HtmlAgilityPack;
using System.Reflection;
using TeamTactics.Webscraper.Attributes;

namespace TeamTactics.Webscraper.Scraper;

internal class WebScraper
{
    private readonly HttpClient _httpClient;
    private readonly ScraperParameters _parameters;
    public WebScraper(HttpClient httpClient, ScraperParameters parameters = null)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36");
        _parameters = parameters ?? new ScraperParameters();
    }

    /// <summary>
    /// Used for scraping list of T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rowXPath"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="HttpProtocolException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="UriFormatException"></exception>
    public async Task<List<T>> ScrapeListAsync<T>() where T : new()
    {

        Type modelType = typeof(T);
        var sourceAttr = modelType.GetCustomAttribute<ScraperSourceAttribute>();
        if (sourceAttr == null)
            throw new InvalidOperationException($"The model {modelType.Name} doesn't have a ScraperSourceAttribute.");


        string url = _parameters.ApplyToTemplate(sourceAttr.UrlTemplate);
        string rowsXPath = _parameters.ApplyToTemplate(sourceAttr.RowsXPathTemplate);

        if(_httpClient.BaseAddress != null)
        {
            url = url.Replace(":", "%3A").Replace("/", "%2F");
            url = _httpClient.BaseAddress + url;
        }

        var htmlDoc = new HtmlDocument();
        try
        {
            string html = await _httpClient.GetStringAsync(url);
            htmlDoc.LoadHtml(html);

        }
        catch (Exception e)
        {
            throw;
        }

        var items = new List<T>();

        var rows = htmlDoc.DocumentNode.SelectNodes(rowsXPath);

        if (rows != null)
        {
            foreach (var row in rows)
            {
                T item = new T();

                var properties = modelType.GetProperties()
                    .Where(p => p.GetCustomAttribute<SelectorAttribute>() != null);

                foreach (var property in properties)
                {
                    var selector = property.GetCustomAttribute<SelectorAttribute>();

                    if (!string.IsNullOrEmpty(selector.XPath))
                    {
                        var node = row.SelectSingleNode(selector.XPath);
                        if (node != null)
                        {
                            string value;
                            if (!string.IsNullOrEmpty(selector.Attribute))
                                value = node.GetAttributeValue(selector.Attribute, "");
                            else
                                value = node.InnerText.Trim();

                            if (!string.IsNullOrEmpty(selector.Transform))
                                value = string.Format(selector.Transform, value);

                            SetPropertyValue(property, item, value);
                        }
                    }
                }

                items.Add(item);
            }
        }

        return items;
    }



    /// <summary>
    /// Used for handling different property types 
    /// </summary>
    /// <param name="property"></param>
    /// <param name="target"></param>
    /// <param name="value"></param>
    private void SetPropertyValue(PropertyInfo property, object target, string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        object convertedValue = null;
        Type propertyType = property.PropertyType;

        if (propertyType == typeof(string))
        {
            convertedValue = value;
        }
        else if (propertyType == typeof(int) && int.TryParse(value, out int intValue))
        {
            convertedValue = intValue;
        }
        else if (propertyType == typeof(decimal) && decimal.TryParse(value, out decimal decimalValue))
        {
            convertedValue = decimalValue;
        }
        else if (propertyType == typeof(DateTime) && DateTime.TryParse(value, out DateTime dateTimeValue))
        {
            convertedValue = dateTimeValue;
        }

        if (convertedValue != null)
        {
            property.SetValue(target, convertedValue);
        }
    }
}
