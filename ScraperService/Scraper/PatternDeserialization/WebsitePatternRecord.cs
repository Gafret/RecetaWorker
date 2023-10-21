using System.Text.Json.Serialization;

namespace RecipeFetcherService.Scraper.PatternDeserialization;

public struct WebsitePatternRecord
{
    [JsonPropertyName("website")]
    public string Website
    {
        get;
        set;
    }

    [JsonPropertyName("recipe_link_pattern")]
    public TagPattern LinkPattern
    {
        get;
        set;
    }
    
    [JsonPropertyName("ingredient_list_pattern")]
    public TagPattern IngredientsPattern
    {
        get;
        set;
    }

    [JsonPropertyName("image_pattern")]
    public TagPattern ImagePattern
    {
        get;
        set;
    }

    public override string ToString()
    {
        return $"{Website}\n{LinkPattern}\n{IngredientsPattern}\n{ImagePattern}";
    }
}