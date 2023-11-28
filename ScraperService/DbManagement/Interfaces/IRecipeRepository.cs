using RecipeFetcherService.DbManagement.DataClasses;
using RecipeFetcherService.DbManagement.DataClasses.Abstract;

namespace RecipeFetcherService.DbManagement.Interfaces;

public interface IRecipeRepository<T, U> : IRepository<T> 
    where T : RecipeBase
    where U : IngredientBase
{
    public T? GetByTitle(string recipeTitle);
    public T? GetByUrl(string url);
    public IEnumerable<U> GetRecipeIngredientsById(int id);

    public IEnumerable<U> GetRecipeIngredientsByTitle(string recipeTitle);

    public IEnumerable<U> GetRecipeIngredientsByUrl(string recipeUrl);
    public void InsertReferencesForIngredients(int recipeId, IEnumerable<int> ingredientIds);
}