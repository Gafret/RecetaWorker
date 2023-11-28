namespace RecipeFetcherService.DbManagement.DataClasses.Abstract;

public abstract class IngredientBase : IEquatable<IngredientBase>
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

    public bool Equals(IngredientBase? other)
    {
        if (other is null) return false;
        return Name == other.Name;
    }

    public override string ToString()
    {
        return Name;
    }
}