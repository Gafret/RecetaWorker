using System.Text.Json.Serialization;
using RecipeFetcherService.Scraper.Interfaces;

namespace RecipeFetcherService.Scraper.PatternDeserialization;

public struct TagPattern : ITagPattern
{
    [JsonPropertyName("tag_name")]
    public string TagName
    {
        get;
        set;
    }
    [JsonPropertyName("required_attrs")]
    public Dictionary<string, string> RequiredAttributes
    {
        get;
        set;
    }

    public override string ToString()
    {
        return $"{TagName}\n";
    }
}