namespace TeamTactics.Webscraper.Attributes;

internal class SelectorAttribute : Attribute
{
    public string XPath { get; }
    public string Attribute { get; }
    public string Transform { get; }

    public SelectorAttribute(string xpath = null, string attribute = null, string transform = null)
    {
        XPath = xpath;
        Attribute = attribute;
        Transform = transform;
    }
}

