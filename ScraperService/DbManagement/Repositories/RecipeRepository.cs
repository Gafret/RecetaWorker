using RecipeFetcherService.DbManagement.DataClasses;

using Dapper;

using System.Data;


namespace RecipeFetcherService.DbManagement.Repositories;

public class RecipeRepository : IRepository<Recipe>
{
    private readonly IDbConnection _dbConnection;

    public RecipeRepository(IDbConnection connection)
    {
        _dbConnection = connection;
    }

    public async Task<IEnumerable<Recipe>> GetAllAsync()
    {
        IEnumerable<Recipe> results = await _dbConnection.QueryAsync<Recipe>("SELECT recipe_id, url FROM recipes");
        return results;
    }
    
    public IEnumerable<Recipe> GetAll()
    {
        IEnumerable<Recipe> results = _dbConnection.Query<Recipe>("SELECT recipe_id, url FROM recipes");
        return results;
    }

    public async Task<Recipe> GetByIdAsync(int id)
    {
        var @params = new { id = id };
        Recipe recipe = (await _dbConnection.QueryAsync<Recipe>(
            "SELECT recipe_id, url FROM recipes WHERE recipe_id = @id", 
            @params)).Single();
        
        return recipe;
    }
    public Recipe GetById(int id)
    {
        var @params = new { Id = id };
        Recipe recipe = _dbConnection.Query<Recipe>(
            "SELECT recipe_id, url FROM recipes WHERE recipe_id = @Id", 
            @params).Single();
        
        return recipe;
    }

    public async Task<bool> CheckIfExistsAsync(string recipeUrl)
    {
        var @params = new { Url = recipeUrl };
        bool recipeExists = (await _dbConnection.QueryAsync(
            "SELECT recipe_id FROM recipes WHERE url = @Url", @params)).Any();
        return recipeExists;
    }

    public bool CheckIfExists(string recipeUrl)
    {
        var @params = new { Url = recipeUrl };
        bool recipeExists = _dbConnection.Query(
            "SELECT recipe_id FROM recipes WHERE url = @Url", @params).Any();
        return recipeExists;
    }

    public async Task<IEnumerable<Ingredient>> GetRecipeIngredientsAsync(int id)
    {
        var @params = new { Id = id };
        IEnumerable<Ingredient> ingredients = await _dbConnection.QueryAsync<Ingredient>(
            "SELECT ing.id, ing.name FROM recipes as rec JOIN recipe_ingredients as rec_ing ON rec.recipe_id = rec_ing.recipe_id " +
            "JOIN ingredients as ing ON rec_ing.ingredient_id = ing.ingredient_id WHERE rec.recipe_id = @Id", @params);
        
        return ingredients;
    }
    
    public IEnumerable<Ingredient> GetRecipeIngredients(int id)
    {
        var @params = new { Id = id };
        IEnumerable<Ingredient> ingredients = _dbConnection.Query<Ingredient>(
            "SELECT ing.name FROM recipes as rec JOIN recipe_ingredients as rec_ing ON rec.recipe_id = rec_ing.recipe_id " +
            "JOIN ingredients as ing ON rec_ing.ingredient_id = ing.ingredient_id WHERE rec.recipe_id = @Id", @params);
        
        return ingredients;
    }

    public async Task InsertAsync(Recipe item)
    {
        await _dbConnection.QueryAsync(
            "INSERT INTO recipes(url) VALUES (@URL)", item);
    }
    
    public void Insert(Recipe item)
    {
        _dbConnection.Query(
            "INSERT INTO recipes(url) VALUES (@Url)", item);
    }


    public async Task UpdateAsync(Recipe item)
    {
        await _dbConnection.QueryAsync(
            "UPDATE recipes SET url = @Url WHERE recipe_id = @RecipeId", item);
    }
    
    public void Update(Recipe item)
    {
        _dbConnection.Query(
            "UPDATE recipes SET url = @Url WHERE recipe_id = @RecipeId", item);
    }

    public async Task DeleteAsync(int id)
    {
        var @params = new { Id = id };
        await _dbConnection.QueryAsync(
            "DELETE FROM recipes WHERE recipe_id = @Id", @params);
    }
    
    public void Delete(int id)
    {
        var @params = new { Id = id };
        _dbConnection.Query(
            "DELETE FROM recipes WHERE recipe_id = @Id", @params);
    }
    
    public void Delete(string url)
    {
        var @params = new { Url = url };
        _dbConnection.Query(
            "DELETE FROM recipes WHERE url = @Url", @params);
    }

    public void Save()
    {
        throw new NotImplementedException();
    }
}