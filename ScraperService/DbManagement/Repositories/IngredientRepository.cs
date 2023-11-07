using System.Data;
using Dapper;
using RecipeFetcherService.DbManagement.DataClasses;
using RecipeFetcherService.DbManagement.Interfaces;

namespace RecipeFetcherService.DbManagement.Repositories;

public class IngredientRepository : IRepository<Ingredient>
{
    private readonly IDbConnection _dbConnection;
    public IngredientRepository(IDbConnection  connection)
    {
        _dbConnection = connection;
    }
    
    public async Task<IEnumerable<Ingredient>> GetAllAsync()
    {
        IEnumerable<Ingredient> ingredients = await _dbConnection.QueryAsync<Ingredient>("SELECT ingredient_id, name FROM ingredients");
        return ingredients;
    }
    
    public IEnumerable<Ingredient> GetAll()
    {
        IEnumerable<Ingredient> ingredients = _dbConnection.Query<Ingredient>("SELECT ingredient_id, name FROM ingredients");
        return ingredients;
    }
    
    public Ingredient? GetByName(string name)
    {
        return _dbConnection.Query<Ingredient>(
            "SELECT ingredient_id, name FROM ingredients WHERE name = @Name",
            new {Name = name}).SingleOrDefault();
    }
    
    public async Task<Ingredient?> GetByNameAsync(string name)
    {
        return  (await _dbConnection.QueryAsync<Ingredient>(
            "SELECT ingredient_id, name FROM ingredients WHERE name = @Name",
            new {Name = name})).SingleOrDefault();
    }

    public IEnumerable<Recipe> GetAssociatedRecipesForIngredient(int ingredientId)
    {
        IEnumerable<Recipe> recipes = _dbConnection.Query<Recipe>(
            "SELECT rec.recipe_id, rec.title, rec.url, rec.image_url FROM recipes AS rec " +
            " JOIN recipe_ingredients AS rec_ingr ON rec.recipe_id = rec_ingr.recipe_id " +
            " JOIN ingredients AS ingr ON rec_ingr.ingredient_id = ingr.ingredient_id WHERE ingr.ingredient_id = @Id",
            new { Id = ingredientId });
        return recipes;
    }
    
    public async Task<IEnumerable<Recipe>> GetAssociatedRecipesForIngredientAsync(int ingredientId)
    {
        IEnumerable<Recipe> recipes = await _dbConnection.QueryAsync<Recipe>(
            "SELECT rec.recipe_id, rec.title, rec.url, rec.image_url FROM recipes AS rec " +
            " JOIN recipe_ingredients AS rec_ingr ON rec.recipe_id = rec_ingr.recipe_id " +
            " JOIN ingredients AS ingr ON rec_ingr.ingredient_id = ingr.ingredient_id WHERE ingr.ingredient_id = @Id",
            new { Id = ingredientId });
        return recipes;
    }

    public IEnumerable<Recipe> GetAssociatedRecipesForIngredientList(IEnumerable<int> ingredientIds)
    {
        IEnumerable<Recipe> recipes = _dbConnection.Query<Recipe>(
            "SELECT DISTINCT rec.recipe_id, rec.title, rec.url, rec.image_url FROM recipes AS rec " +
            " JOIN recipe_ingredients AS rec_ingr ON rec.recipe_id = rec_ingr.recipe_id " +
            " JOIN ingredients AS ingr ON rec_ingr.ingredient_id = ingr.ingredient_id WHERE ingr.ingredient_id = ANY(:IngredientIds)", 
            new {IngredientIds = ingredientIds});
        return recipes;
    }
    
    public async Task<IEnumerable<Recipe>> GetAssociatedRecipesForIngredientListAsync(IEnumerable<int> ingredientIds)
    {
        IEnumerable<Recipe> recipes = await _dbConnection.QueryAsync<Recipe>(
            "SELECT DISTINCT rec.recipe_id, rec.title, rec.url, rec.image_url FROM recipes AS rec " +
            " JOIN recipe_ingredients AS rec_ingr ON rec.recipe_id = rec_ingr.recipe_id " +
            " JOIN ingredients AS ingr ON rec_ingr.ingredient_id = ingr.ingredient_id WHERE ingr.ingredient_id = ANY(:IngredientIds)", 
            new {IngredientIds = ingredientIds});
        return recipes;
    }

    public IEnumerable<int> InsertMany(IEnumerable<Ingredient> ingredients)
    {
        string bulkInsert = "";
        foreach (var ingredient in ingredients)
        {
            bulkInsert += $"('{ingredient.Name}'), ";
        }

        bulkInsert = bulkInsert.Substring(0, bulkInsert.Length - 2);

        IEnumerable<int> insertedIngredients = _dbConnection.Query<int>(
            $"INSERT INTO ingredients(name) VALUES {bulkInsert} RETURNING ingredient_id");

        return insertedIngredients;
    }
    
    public async Task<IEnumerable<int>> InsertManyAsync(IEnumerable<Ingredient> ingredients)
    {
        string bulkInsert = "";
        foreach (var ingredient in ingredients)
        {
            bulkInsert += $"('{ingredient.Name}'), ";
        }

        bulkInsert = bulkInsert.Substring(0, bulkInsert.Length - 2);

        IEnumerable<int> insertedIngredients = await _dbConnection.QueryAsync<int>(
            $"INSERT INTO ingredients(name) VALUES {bulkInsert} RETURNING ingredient_id");

        return insertedIngredients;
    }


    public async Task<Ingredient?> GetByIdAsync(int id)
    {
        var @params = new { Id = id };
        Ingredient? ingredient = (await _dbConnection.QueryAsync<Ingredient>(
            "SELECT ingredient_id, name FROM ingredients WHERE ingredient_id = @Id", 
            @params)).SingleOrDefault();
        
        return ingredient;
    }
    
    public Ingredient? GetById(int id)
    {
        var @params = new { Id = id };
        Ingredient? ingredient = _dbConnection.Query<Ingredient>(
            "SELECT ingredient_id, name FROM ingredients WHERE ingredient_id = @Id", 
            @params).SingleOrDefault();
        
        return ingredient;
    }

    public async Task<int> InsertAsync(Ingredient item)
    {
        return (await _dbConnection.QueryAsync<int>(
            "INSERT INTO ingredients(name) VALUES (@Name) RETURNING ingredient_id", item)).Single();
    }
    
    public int Insert(Ingredient item)
    {
        return _dbConnection.Query<int>(
            "INSERT INTO ingredients(name) VALUES (@Name) RETURNING ingredient_id", item).Single();
    }

    public async Task UpdateAsync(Ingredient item)
    {
        await _dbConnection.QueryAsync(
            "UPDATE ingredients SET name = @Name WHERE ingredient_id = @IngredientId", item);
    }
    
    public void Update(Ingredient item)
    {
        _dbConnection.Query(
            "UPDATE ingredients SET name = @Name WHERE ingredient_id = @IngredientId", item);
    }

    public async Task DeleteAsync(int id)
    {
        var @params = new { Id = id };
        await _dbConnection.QueryAsync(
            "DELETE FROM ingredients WHERE ingredient_id = @Id", @params);
    }
    public void Delete(int id)
    {
        var @params = new { Id = id };
        _dbConnection.Query(
            "DELETE FROM ingredients WHERE ingredient_id = @Id", @params);
    }
}