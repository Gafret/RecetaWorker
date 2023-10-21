namespace RecipeFetcherService.Scraper.Interfaces;

public interface IPatternReader<T>
{
    IEnumerable<T> ReadPatterns(string filePath);
}