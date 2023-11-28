using RecipeFetcherService.DbManagement.DataClasses;

namespace RecipeFetcherService.DbManagement.Interfaces;

public interface IIngredientRepository<T, U> : IRepository<T> 
    where T : Ingredient 
    where U : Recipe
{
    public T? GetByName(string name);
    public IEnumerable<U> GetAssociatedRecipesForIngredient(int ingredientId);
    public IEnumerable<U> GetAssociatedRecipesForIngredientList(IEnumerable<int> ingredientIds);
    public IEnumerable<int> InsertMany(IEnumerable<T> ingredients);
}