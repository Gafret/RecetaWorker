namespace RecipeFetcherService.DbManagement.DataClasses;

public class Recipe : IEquatable<Recipe>
{
    public int RecipeId
    {
        get;
        set;
    }

    public string Title
    {
        get;
        set;
    }
    
    public string Url
    {
        get;
        set;
    }
    
    public string ImageUrl
    {
        get;
        set;
    }

    public bool Equals(Recipe? other)
    {
        if (other?.Title == Title &&
            other?.Url == Url &&
            other?.ImageUrl == ImageUrl)
            return true;
        
        return false;
    }

    public override string ToString()
    {
        return Url;
    }
}