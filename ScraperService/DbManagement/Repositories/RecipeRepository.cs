using RecipeFetcherService.DbManagement.DataClasses;

using Dapper;

using System.Data;
using RecipeFetcherService.DbManagement.Interfaces;


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
        IEnumerable<Recipe> results = await _dbConnection.QueryAsync<Recipe>("SELECT * FROM recipes");
        return results;
    }
    
    public IEnumerable<Recipe> GetAll()
    {
        IEnumerable<Recipe> results = _dbConnection.Query<Recipe>("SELECT * FROM recipes");
        return results;
    }
    
    public async Task<Recipe?> GetByTitleAsync(string recipeTitle)
    {
        var @params = new { Title = recipeTitle };
        Recipe? recipe = (await _dbConnection.QueryAsync<Recipe>(
            "SELECT * FROM recipes WHERE title = @Title", 
            @params)).SingleOrDefault();
        
        return recipe;
    }
    public Recipe? GetByTitle(string recipeTitle)
    {
        var @params = new { Title = recipeTitle };
        Recipe? recipe = _dbConnection.Query<Recipe>(
            "SELECT * FROM recipes WHERE title = @Title", 
            @params).SingleOrDefault();
        
        return recipe;
    }

    public async Task<Recipe?> GetByIdAsync(int id)
    {
        var @params = new { Id = id };
        Recipe? recipe = (await _dbConnection.QueryAsync<Recipe>(
            "SELECT * FROM recipes WHERE recipe_id = @Id", 
            @params)).SingleOrDefault();
        
        return recipe;
    }
    public Recipe? GetById(int id)
    {
        var @params = new { Id = id };
        Recipe? recipe = _dbConnection.Query<Recipe>(
            "SELECT * FROM recipes WHERE recipe_id = @Id", 
            @params).SingleOrDefault();
        
        return recipe;
    }
    
    public async Task<Recipe?> GetByUrlAsync(string url)
    {
        var @params = new { Url = url };
        Recipe? recipe = (await _dbConnection.QueryAsync<Recipe>(
            "SELECT * FROM recipes WHERE url = @Url",
            @params)).SingleOrDefault();
        return recipe;
    }

    public Recipe? GetByUrl(string url)
    {
        var @params = new { Url = url };
        Recipe? recipe = _dbConnection.Query<Recipe>(
            "SELECT * FROM recipes WHERE url = @Url",
            @params).SingleOrDefault();
        return recipe;
    }

    public async Task<IEnumerable<Ingredient>> GetRecipeIngredientsByIdAsync(int id)
    {
        var @params = new { Id = id };
        IEnumerable<Ingredient> ingredients = await _dbConnection.QueryAsync<Ingredient>(
            "SELECT ing.id, ing.name FROM recipes as rec JOIN recipe_ingredients as rec_ing ON rec.recipe_id = rec_ing.recipe_id " +
            "JOIN ingredients as ing ON rec_ing.ingredient_id = ing.ingredient_id WHERE rec.recipe_id = @Id", @params);
        
        return ingredients;
    }
    
    public IEnumerable<Ingredient> GetRecipeIngredientsById(int id)
    {
        var @params = new { Id = id };
        IEnumerable<Ingredient> ingredients = _dbConnection.Query<Ingredient>(
            "SELECT ing.name FROM recipes as rec JOIN recipe_ingredients as rec_ing ON rec.recipe_id = rec_ing.recipe_id " +
            "JOIN ingredients as ing ON rec_ing.ingredient_id = ing.ingredient_id WHERE rec.recipe_id = @Id", @params);
        
        return ingredients;
    }
    
    public async Task<IEnumerable<Ingredient>> GetRecipeIngredientsByTitleAsync(string recipeTitle)
    {
        var @params = new { Title = recipeTitle };
        IEnumerable<Ingredient> ingredients = await _dbConnection.QueryAsync<Ingredient>(
            "SELECT ing.name FROM recipes as rec JOIN recipe_ingredients as rec_ing ON rec.recipe_id = rec_ing.recipe_id " +
            "JOIN ingredients as ing ON rec_ing.ingredient_id = ing.ingredient_id WHERE rec.title = @Title", @params);
        
        return ingredients;
    }
    
    public IEnumerable<Ingredient> GetRecipeIngredientsByTitle(string recipeTitle)
    {
        var @params = new { Title = recipeTitle };
        IEnumerable<Ingredient> ingredients = _dbConnection.Query<Ingredient>(
            "SELECT ing.name FROM recipes as rec JOIN recipe_ingredients as rec_ing ON rec.recipe_id = rec_ing.recipe_id " +
            "JOIN ingredients as ing ON rec_ing.ingredient_id = ing.ingredient_id WHERE rec.title = @Title", @params);
        
        return ingredients;
    }
    
    public async Task<IEnumerable<Ingredient>> GetRecipeIngredientsByUrlAsync(string recipeUrl)
    {
        var @params = new { Url = recipeUrl };
        IEnumerable<Ingredient> ingredients = await _dbConnection.QueryAsync<Ingredient>(
            "SELECT ing.name FROM recipes as rec JOIN recipe_ingredients as rec_ing ON rec.recipe_id = rec_ing.recipe_id " +
            "JOIN ingredients as ing ON rec_ing.ingredient_id = ing.ingredient_id WHERE rec.url = @Url", @params);
        
        return ingredients;
    }
    
    public IEnumerable<Ingredient> GetRecipeIngredientsByUrl(string recipeUrl)
    {
        var @params = new { Url = recipeUrl };
        IEnumerable<Ingredient> ingredients = _dbConnection.Query<Ingredient>(
            "SELECT ing.name FROM recipes as rec JOIN recipe_ingredients as rec_ing ON rec.recipe_id = rec_ing.recipe_id " +
            "JOIN ingredients as ing ON rec_ing.ingredient_id = ing.ingredient_id WHERE rec.url = @Url", @params);
        
        return ingredients;
    }
    
    public async Task InsertReferencesForIngredientsAsync(int recipeId, IEnumerable<int> ingredientIds)
    {
        string bulkInsert = "";
        foreach (var ingredientId in ingredientIds)
        {
            bulkInsert += $"('{recipeId}', '{ingredientId}'),";
        }

        bulkInsert = bulkInsert.Substring(0, bulkInsert.Length - 1);
        await _dbConnection.QueryAsync(
            $"INSERT INTO recipe_ingredients VALUES {bulkInsert}");
    }
    
    public void InsertReferencesForIngredients(int recipeId, IEnumerable<int> ingredientIds)
    {
        string bulkInsert = "";
        foreach (var ingredientId in ingredientIds)
        {
            bulkInsert += $"('{recipeId}', '{ingredientId}'),";
        }

        bulkInsert = bulkInsert.Substring(0, bulkInsert.Length - 1);
        _dbConnection.Query(
            $"INSERT INTO recipe_ingredients VALUES {bulkInsert}");
    }
    
    public async Task<int> InsertAsync(Recipe item)
    {
        return (await _dbConnection.QueryAsync<int>(
            "INSERT INTO recipes(url, title, image_url) VALUES (@Url, @Title, @ImageUrl) RETURNING recipe_id", item)).Single();
    }
    
    public int Insert(Recipe item)
    {
        return _dbConnection.Query<int>(
            "INSERT INTO recipes(url, title, image_url) VALUES (@Url, @Title, @ImageUrl) RETURNING recipe_id", item).Single();
    }


    public async Task UpdateAsync(Recipe item)
    {
        await _dbConnection.QueryAsync(
            "UPDATE recipes SET url = @Url, title = @Title, image_url = @ImageUrl WHERE recipe_id = @RecipeId", item);
    }
    
    public void Update(Recipe item)
    {
        _dbConnection.Query(
            "UPDATE recipes SET url = @Url, title = @Title, image_url = @ImageUrl WHERE recipe_id = @RecipeId", item);
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
}