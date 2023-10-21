namespace RecipeFetcherService.DbManagement.DataClasses;

public class Recipe
{
    public int RecipeId
    {
        get;
        set;
    }
    
    public string Url
    {
        get;
        set;
    }

    public override string ToString()
    {
        return Url;
    }
}