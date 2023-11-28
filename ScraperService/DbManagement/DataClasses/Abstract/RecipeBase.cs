namespace RecipeFetcherService.DbManagement.DataClasses.Abstract;

public abstract class RecipeBase : IEquatable<RecipeBase>
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

    public string? ImageUrl
    {
        get;
        set;
    } 

    public bool Equals(RecipeBase? other)
    {
        if (other is null)
            return false;
        
        if (other.Title == Title &&
            other.Url == Url &&
            other.ImageUrl == ImageUrl)
            return true;
        
        return false;
    }

    public override string ToString()
    {
        return Url;
    }
}