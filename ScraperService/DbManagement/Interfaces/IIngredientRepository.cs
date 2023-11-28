using RecipeFetcherService.DbManagement.DataClasses;
using RecipeFetcherService.DbManagement.DataClasses.Abstract;

namespace RecipeFetcherService.DbManagement.Interfaces;

public interface IIngredientRepository<T, U> : IRepository<T> 
    where T : IngredientBase
    where U : RecipeBase
{
    public T? GetByName(string name);
    public IEnumerable<U> GetAssociatedRecipesForIngredient(int ingredientId);
    public IEnumerable<U> GetAssociatedRecipesForIngredientList(IEnumerable<int> ingredientIds);
    public IEnumerable<int> InsertMany(IEnumerable<T> ingredients);
}