namespace RecipeFetcherService.Scraper.Interfaces;

public interface ITagPattern
{
    string TagName
    {
        get;
        set;
    }

    Dictionary<string, string> RequiredAttributes
    {
        get;
        set;
    }
}