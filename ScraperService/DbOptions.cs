namespace RecipeFetcherService;

public class DbOptions
{
    public const string ConnectionStrings = "ConnectionStrings";
    public string Connection
    {
        get;
        set;
    } = String.Empty;
}