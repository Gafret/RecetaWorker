using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RecipeFetcherService.DbManagement.DataClasses;
using RecipeFetcherService.DbManagement.Repositories;

namespace ScraperService.Tests;

public class IngredientRepositoryTest
{
    private static IDbConnection _dbConnection;
    private static IngredientRepository _ingredientRepository;
    
    static IngredientRepositoryTest()
    {
        IConfiguration config = SetupConfig();
        string connectionString = config["ConnectionStrings:PostgreSqlTest"];
        
        _dbConnection = new NpgsqlConnection(connectionString);
        _ingredientRepository = new IngredientRepository(_dbConnection);
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private static IConfiguration SetupConfig()
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        IConfiguration config = builder.Build();

        return config;
    }
    internal static Ingredient? GetIngredientById(int id)
    {
        return _dbConnection.Query<Ingredient>($"SELECT * FROM ingredients WHERE ingredient_id = {id}")
            .SingleOrDefault();
    }
    
    internal static IEnumerable<Ingredient> GetAllIngredients()
    {
        return _dbConnection.Query<Ingredient>($"SELECT * FROM ingredients");
    }
    internal static List<int> InsertManyIngredientsToTable(IEnumerable<Ingredient> ingredients)
    {
        List<int> ids = new List<int>();
        foreach (var ingredient in ingredients)
        {
            ids.Add(InsertIngredientToTable(ingredient));
        }

        return ids;
    }
    
    internal static int InsertIngredientToTable(Ingredient ingredient)
    {
        return _dbConnection
            .Query<int>(
                "INSERT INTO ingredients(name)  VALUES(@Name) RETURNING ingredient_id;",
                ingredient).SingleOrDefault();
    }

    internal static void DeleteManyIngredients(IEnumerable<int> ids)
    {
        foreach (var id in ids)
        {
            DeleteIngredient(id);
        }
    }

    internal static void DeleteIngredient(int id)
    {
        _dbConnection.Query($"DELETE FROM ingredients WHERE ingredient_id = {id}");
    }

    internal static void InsertAssociationRecipeIngredient(int recipeId, int ingredientId)
    {
        _dbConnection.Query("INSERT INTO public.recipe_ingredients(recipe_id, ingredient_id) VALUES(@RecId, @IngId);",
            new { RecId = recipeId, IngId = ingredientId });
    }

    internal static void DeleteAssociationRecipeIngredient(int recipeId, int ingredientId)
    {
        _dbConnection.Query("DELETE FROM public.recipe_ingredients WHERE recipe_id = @RecId AND ingredient_id = @IngId;",
            new { RecId = recipeId, IngId = ingredientId });
    }

    [Fact]
    public void GetAll_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        Ingredient ingredient1 = new Ingredient()
        {
            Name = "Coke1"
        };
        
        List<int> ids = InsertManyIngredientsToTable(new Ingredient[] { ingredient, ingredient1 });

        IEnumerable<Ingredient> ingredients = _ingredientRepository.GetAll();
        DeleteManyIngredients(ids);
        
        Assert.Equal(new Ingredient[] { ingredient, ingredient1 }, ingredients);
    }
    
    [Fact]
    public async Task GetAllAsync_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        Ingredient ingredient1 = new Ingredient()
        {
            Name = "Coke1"
        };
        List<int> ids = InsertManyIngredientsToTable(new [] { ingredient, ingredient1 });

        IEnumerable<Ingredient> ingredients = await _ingredientRepository.GetAllAsync();
        DeleteManyIngredients(ids);
        
        Assert.Equal(new [] { ingredient, ingredient1 }, ingredients);
    }
    
    [Fact]
    public void GetByName_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        int id = InsertIngredientToTable(ingredient);

        Ingredient result = _ingredientRepository.GetByName("Coke");
        DeleteIngredient(id);
        
        Assert.Equal(ingredient, result);

    }
    
    [Fact]
    public async Task GetByNameAsync_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        int id = InsertIngredientToTable(ingredient);

        Ingredient result = await _ingredientRepository.GetByNameAsync("Coke");
        DeleteIngredient(id);
        
        Assert.Equal(ingredient, result);
    }
    
    [Fact]
    public void GetAssociatedRecipesForIngredient_DataExist_ReturnTrue()
    {
        Recipe r1 = new Recipe()
        {
            Title = "google",
            Url = "google",
            ImageUrl = "google",
        };
        Recipe r2 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        List<int> recIds = RecipeRepositoryTest.InsertManyRecipesToTable(new Recipe[]{r1, r2});
        int ingId = InsertIngredientToTable(ingredient);        
        InsertAssociationRecipeIngredient(recIds[0], ingId);
        InsertAssociationRecipeIngredient(recIds[1], ingId);

        IEnumerable<Recipe> recipes = _ingredientRepository.GetAssociatedRecipesForIngredient(ingId);
        
        DeleteIngredient(ingId);
        RecipeRepositoryTest.DeleteRecipe(recIds[0]);
        RecipeRepositoryTest.DeleteRecipe(recIds[1]);
        DeleteAssociationRecipeIngredient(recIds[0], ingId);
        DeleteAssociationRecipeIngredient(recIds[1], ingId);
        Assert.Equal(new Recipe[]{r1, r2}, recipes);
    }
    
    [Fact]
    public async Task GetAssociatedRecipesForIngredientAsync_DataExist_ReturnTrue()
    {
        Recipe r1 = new Recipe()
        {
            Title = "google",
            Url = "google",
            ImageUrl = "google",
        };
        Recipe r2 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        List<int> recIds = RecipeRepositoryTest.InsertManyRecipesToTable(new Recipe[]{r1, r2});
        int ingId = InsertIngredientToTable(ingredient);        
        InsertAssociationRecipeIngredient(recIds[0], ingId);
        InsertAssociationRecipeIngredient(recIds[1], ingId);

        IEnumerable<Recipe> recipes = await _ingredientRepository.GetAssociatedRecipesForIngredientAsync(ingId);
        
        DeleteIngredient(ingId);
        RecipeRepositoryTest.DeleteRecipe(recIds[0]);
        RecipeRepositoryTest.DeleteRecipe(recIds[1]);
        DeleteAssociationRecipeIngredient(recIds[0], ingId);
        DeleteAssociationRecipeIngredient(recIds[1], ingId);
        Assert.Equal(new Recipe[]{r1, r2}, recipes);
    }
    
    [Fact]
    public void GetAssociatedRecipesForIngredientList_DataExist_ReturnTrue()
    {
        Recipe r1 = new Recipe()
        {
            Title = "google",
            Url = "google",
            ImageUrl = "google",
        };
        Recipe r2 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        Ingredient i1 = new Ingredient()
        {
            Name = "Coke"
        };
        Ingredient i2 = new Ingredient()
        {
            Name = "Coke2"
        };
        List<int> recIds = RecipeRepositoryTest.InsertManyRecipesToTable(new []{r1, r2});
        List<int> ingIds = InsertManyIngredientsToTable(new [] { i1, i2 });
        InsertAssociationRecipeIngredient(recIds[0], ingIds[0]);
        InsertAssociationRecipeIngredient(recIds[0], ingIds[1]);
        InsertAssociationRecipeIngredient(recIds[1], ingIds[0]);
        InsertAssociationRecipeIngredient(recIds[1], ingIds[1]);

        IEnumerable<Recipe> recipes = _ingredientRepository.GetAssociatedRecipesForIngredientList(ingIds);
        
        DeleteIngredient(ingIds[0]);
        DeleteIngredient(ingIds[1]);
        RecipeRepositoryTest.DeleteRecipe(recIds[0]);
        RecipeRepositoryTest.DeleteRecipe(recIds[1]);
        DeleteAssociationRecipeIngredient(recIds[0], ingIds[0]);
        DeleteAssociationRecipeIngredient(recIds[0], ingIds[1]);
        DeleteAssociationRecipeIngredient(recIds[1], ingIds[0]);
        DeleteAssociationRecipeIngredient(recIds[1], ingIds[1]);
        Assert.Equal(new Recipe[]{r1, r2}, recipes.OrderBy((recipe => recipe.Title)));
    }
    
    [Fact]
    public async Task GetAssociatedRecipesForIngredientListAsync_DataExist_ReturnTrue()
    {
        Recipe r1 = new Recipe()
        {
            Title = "google",
            Url = "google",
            ImageUrl = "google",
        };
        Recipe r2 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        Ingredient i1 = new Ingredient()
        {
            Name = "Coke"
        };
        Ingredient i2 = new Ingredient()
        {
            Name = "Coke2"
        };
        List<int> recIds = RecipeRepositoryTest.InsertManyRecipesToTable(new []{r1, r2});
        List<int> ingIds = InsertManyIngredientsToTable(new [] { i1, i2 });
        InsertAssociationRecipeIngredient(recIds[0], ingIds[0]);
        InsertAssociationRecipeIngredient(recIds[0], ingIds[1]);
        InsertAssociationRecipeIngredient(recIds[1], ingIds[0]);
        InsertAssociationRecipeIngredient(recIds[1], ingIds[1]);

        IEnumerable<Recipe> recipes = _ingredientRepository.GetAssociatedRecipesForIngredientList(ingIds);
        
        DeleteIngredient(ingIds[0]);
        DeleteIngredient(ingIds[1]);
        RecipeRepositoryTest.DeleteRecipe(recIds[0]);
        RecipeRepositoryTest.DeleteRecipe(recIds[1]);
        DeleteAssociationRecipeIngredient(recIds[0], ingIds[0]);
        DeleteAssociationRecipeIngredient(recIds[0], ingIds[1]);
        DeleteAssociationRecipeIngredient(recIds[1], ingIds[0]);
        DeleteAssociationRecipeIngredient(recIds[1], ingIds[1]);
        Assert.Equal(new []{r1, r2}, recipes.OrderBy((recipe => recipe.Title)));
    }
    
    [Fact]
    public void InsertMany_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        Ingredient ingredient1 = new Ingredient()
        {
            Name = "Coke1"
        };
        Ingredient[] ingredients = new[] { ingredient, ingredient1 };

        IEnumerable<int> ids = _ingredientRepository.InsertMany(ingredients);
        
        IEnumerable<Ingredient> result = GetAllIngredients();
        DeleteManyIngredients(ids);
        Assert.Equal(ingredients, result);
    }
    
    [Fact]
    public async Task InsertManyAsync_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        Ingredient ingredient1 = new Ingredient()
        {
            Name = "Coke1"
        };
        Ingredient[] ingredients = new[] { ingredient, ingredient1 };

        IEnumerable<int> ids = await _ingredientRepository.InsertManyAsync(ingredients);
        
        IEnumerable<Ingredient> result = GetAllIngredients();
        DeleteManyIngredients(ids);
        Assert.Equal(ingredients, result);
    }
    
    [Fact]
    public void GetById_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        int id = InsertIngredientToTable(ingredient);

        Ingredient returned = _ingredientRepository.GetById(id);
        
        DeleteIngredient(id);
        Assert.Equal(ingredient, returned);
    }
    
    [Fact]
    public async Task GetByIdAsync_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        int id = InsertIngredientToTable(ingredient);

        Ingredient returned = await _ingredientRepository.GetByIdAsync(id);
        
        DeleteIngredient(id);
        Assert.Equal(ingredient, returned);
    }
    
    [Fact]
    public void Insert_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };

        int ingredientId = _ingredientRepository.Insert(ingredient);

        Ingredient returned = GetIngredientById(ingredientId);
        DeleteIngredient(ingredientId);
        Assert.Equal(ingredient, returned);
    }
    
    [Fact]
    public async Task InsertAsync_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };

        int ingredientId = await _ingredientRepository.InsertAsync(ingredient);

        Ingredient returned = GetIngredientById(ingredientId);
        DeleteIngredient(ingredientId);
        Assert.Equal(ingredient, returned);
    }
    
    [Fact]
    public void Update_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        int ingredientId = InsertIngredientToTable(ingredient);
        Ingredient returned = GetIngredientById(ingredientId);
        returned.Name = "Coke2";
        
        _ingredientRepository.Update(returned);

        Ingredient updatedIngredient = GetIngredientById(ingredientId);
        DeleteIngredient(ingredientId);
        Assert.Equal(returned, updatedIngredient);
    }
    
    [Fact]
    public async Task UpdateAsync_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        int ingredientId = InsertIngredientToTable(ingredient);
        Ingredient returned = GetIngredientById(ingredientId);
        returned.Name = "Coke2";
        
        await _ingredientRepository.UpdateAsync(returned);

        Ingredient updatedIngredient = GetIngredientById(ingredientId);
        DeleteIngredient(ingredientId);
        Assert.Equal(returned, updatedIngredient);
    }
    
    [Fact]
    public void Delete_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        int ingredientId = InsertIngredientToTable(ingredient);
        
        _ingredientRepository.Delete(ingredientId);

        Ingredient returned = GetIngredientById(ingredientId);
        DeleteIngredient(ingredientId);
        Assert.Null(returned);
    }
    
    [Fact]
    public async Task DeleteAsync_DataExist_ReturnTrue()
    {
        Ingredient ingredient = new Ingredient()
        {
            Name = "Coke"
        };
        int ingredientId = InsertIngredientToTable(ingredient);
        
        await _ingredientRepository.DeleteAsync(ingredientId);

        Ingredient returned = GetIngredientById(ingredientId);
        DeleteIngredient(ingredientId);
        Assert.Null(returned);
    }
}