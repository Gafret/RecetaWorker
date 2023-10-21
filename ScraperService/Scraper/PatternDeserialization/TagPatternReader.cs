using System.Text.Json;
using RecipeFetcherService.Scraper.Interfaces;

namespace RecipeFetcherService.Scraper.PatternDeserialization;

public class TagPatternReader : IPatternReader<WebsitePatternRecord>
{
    public IEnumerable<WebsitePatternRecord> ReadPatterns(string filePath)
    {
        string json = File.ReadAllText(filePath);
        IEnumerable<WebsitePatternRecord>? tagPatterns =
            JsonSerializer.Deserialize<IEnumerable<WebsitePatternRecord>>(json);
        return tagPatterns;
    }
}