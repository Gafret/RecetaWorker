using HtmlAgilityPack;
using RecipeFetcherService.Scraper.Interfaces;

namespace RecipeFetcherService.Scraper;

public class WebCrawler
{
    private static readonly HttpClient _client = new HttpClient(
        handler: new SocketsHttpHandler()
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            }
        );
    private static bool CompareAttrs(
        IEnumerable<HtmlAttribute> actualAttributes,
        Dictionary<string, string> requiredAttributes
        )
    {
        Dictionary<string, string> htmlAttributes = actualAttributes.ToDictionary(
            (attribute => attribute.Name), 
            (attribute => attribute.Value)
            );

        foreach (var requiredAttribute in requiredAttributes.Keys)
        {
            string? htmlValue;
            var hasValue = htmlAttributes.TryGetValue(requiredAttribute, out htmlValue);
            // discard tag if there is no required value or values don't match
            if (!hasValue || htmlValue != requiredAttributes[requiredAttribute])
            {
                return false;
            } 
        }

        return true;
    }
    
    private static List<HtmlNode> FilterTagsByAttributes(
        IEnumerable<HtmlNode> tags,
        Dictionary<string, string> requiredAttributes
        )
    {
        List<HtmlNode> filteredTags = new List<HtmlNode>();
        
        foreach (var tag in tags)
        {
            IEnumerable<HtmlAttribute> tagAttributes = tag.GetAttributes();
            if (CompareAttrs(tagAttributes, requiredAttributes))
            {
                filteredTags.Add(tag);
            }
        }

        return filteredTags;
    }

    public static async Task<HtmlDocument> GetHtmlDocument(string url)
    {
        string htmlStr = await _client.GetStringAsync(url);
        
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlStr);

        return htmlDoc;
    }
    
    public static List<HtmlNode>? FindTags(HtmlDocument htmlDoc, string tagName, Dictionary<string, string> attributes)
    {
        IEnumerable<HtmlNode> allTags = htmlDoc.DocumentNode.Descendants(tagName);
        if (allTags is null) return null;
        
        List<HtmlNode> filteredTags = FilterTagsByAttributes(allTags, attributes);

        return filteredTags;
    }
    
    public static List<HtmlNode>? FindTags(HtmlDocument htmlDoc, ITagPattern tagPattern)
    {
        (string tagName, Dictionary<string, string> attributes) = (tagPattern.TagName, tagPattern.RequiredAttributes);
        return FindTags(htmlDoc, tagName, attributes);
    }
}