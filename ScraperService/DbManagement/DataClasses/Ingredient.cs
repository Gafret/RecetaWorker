namespace RecipeFetcherService.DbManagement.DataClasses;

public class Ingredient
{
    public int RecipeId
    {
        get;
        set;
    }
    
    public string Name
    {
        get;
        set;
    }

    public override string ToString()
    {
        return Name;
    }
}