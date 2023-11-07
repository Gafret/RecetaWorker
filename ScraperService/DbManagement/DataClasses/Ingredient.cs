namespace RecipeFetcherService.DbManagement.DataClasses;

public class Ingredient : IEquatable<Ingredient>
{
    public int IngredientId
    {
        get;
        set;
    }
    
    public string Name
    {
        get;
        set;
    }

    public bool Equals(Ingredient? other)
    {
        return Name == other.Name;
    }

    public override string ToString()
    {
        return Name;
    }
}