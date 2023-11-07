using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RecipeFetcherService.DbManagement.DataClasses;
using RecipeFetcherService.DbManagement.Repositories;

namespace ScraperService.Tests;

public class RecipeRepositoryTest
{
    private static IDbConnection _dbConnection;
    private static RecipeRepository _recipeRepository;
    
    static RecipeRepositoryTest()
    {
        IConfiguration config = SetupConfig();
        string connectionString = config["ConnectionStrings:PostgreSqlTest"];
        
        _dbConnection = new NpgsqlConnection(connectionString);
        _recipeRepository = new RecipeRepository(_dbConnection);
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

    internal static List<int> InsertManyRecipesToTable(IEnumerable<Recipe> recipes)
    {
        List<int> ids = new List<int>();
        foreach (var recipe in recipes)
        {
            ids.Add(InsertRecipeToTable(recipe));
        }

        return ids;
    }
    
    internal static int InsertRecipeToTable(Recipe recipe)
    {
        return _dbConnection
            .Query<int>(
                "INSERT INTO recipes(url, title, image_url)  VALUES(@Url, @Title, @ImageUrl) RETURNING recipe_id;",
                recipe).SingleOrDefault();
    }

    internal static void DeleteManyRecipes(IEnumerable<int> ids)
    {
        foreach (var id in ids)
        {
            DeleteRecipe(id);
        }
    }

    internal static void DeleteRecipe(int id)
    {
        _dbConnection.Query($"DELETE FROM recipes WHERE recipe_id = {id}");
    }
    
    [Fact]
    public void GetAll_DataExists_ReturnTrue()
    {
        Recipe recipe1 = new Recipe()
        {
            Title = "google",
            Url = "google",
            ImageUrl = "google",
        };
        
        Recipe recipe2 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        int id1 = InsertRecipeToTable(recipe1);
        int id2 = InsertRecipeToTable(recipe2);
        
        IEnumerable<Recipe> recipes = _recipeRepository.GetAll();
        
        DeleteRecipe(id1);
        DeleteRecipe(id2);
        
        Assert.Equal<Recipe>(new Recipe[]{recipe1, recipe2}, recipes);
    }
    
    [Fact]
    public async Task GetAllAsync_DataExists_ReturnTrue()
    {
        Recipe recipe1 = new Recipe()
        {
            Title = "google",
            Url = "google",
            ImageUrl = "google",
        };
        
        Recipe recipe2 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        int id1 = InsertRecipeToTable(recipe1);
        int id2 = InsertRecipeToTable(recipe2);
        
        
        IEnumerable<Recipe> recipes = await _recipeRepository.GetAllAsync();
        
        DeleteRecipe(id1);
        DeleteRecipe(id2);
    
        
        Assert.Equal<Recipe>(new Recipe[]{recipe1, recipe2}, recipes);
    }
    
    [Fact]
    public void GetById_DataExists_ReturnTrue()
    {
        Recipe recipe1 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        int id1 = InsertRecipeToTable(recipe1);
        
        Recipe recipe = _recipeRepository.GetById(id1);
        
        DeleteRecipe(id1);
        
        Assert.Equal(recipe1, recipe);
    }
    
    [Fact]
    public async Task GetByIdAsync_DataExists_ReturnTrue()
    {
        Recipe recipe1 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        int id1 = InsertRecipeToTable(recipe1);
        
        Recipe recipe = await _recipeRepository.GetByIdAsync(id1);
        
        DeleteRecipe(id1);
        
        Assert.Equal(recipe1, recipe);
    }
    
    [Fact]
    public void GetByTitle_DataExists_ReturnTrue()
    {
        Recipe recipe1 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        int id1 = InsertRecipeToTable(recipe1);
        
        Recipe recipe = _recipeRepository.GetByTitle("google1");
        
        DeleteRecipe(id1);

        
        Assert.Equal(recipe1,recipe);
    }
    
    [Fact]
    public async Task GetByTitleAsync_DataExists_ReturnTrue()
    {
        Recipe recipe1 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        int id1 = InsertRecipeToTable(recipe1);
        
        Recipe recipe = await _recipeRepository.GetByTitleAsync("google1");
        
        DeleteRecipe(id1);

        
        Assert.Equal(recipe1,recipe);
    }
    
    [Fact]
    public void GetByUrl_DataExists_ReturnTrue()
    {
        Recipe recipe1 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        int id1 = InsertRecipeToTable(recipe1);
        
        Recipe recipe = _recipeRepository.GetByUrl("google1");
        
        DeleteRecipe(id1);
        
        Assert.Equal(recipe1,recipe);
    }
    
    [Fact]
    public async Task GetByUrlAsync_DataExists_ReturnTrue()
    {
        Recipe recipe1 = new Recipe()
        {
            Title = "google1",
            Url = "google1",
            ImageUrl = "google1",
        };
        int id1 = InsertRecipeToTable(recipe1);
        
        Recipe recipe = await _recipeRepository.GetByUrlAsync("google1");
        
        DeleteRecipe(id1);
        
        Assert.Equal(recipe1,recipe);
    }
    
    [Fact]
    public void  Insert_ReturnTrue()
    {
        Recipe newRecipe = new Recipe()
        {
            Url = "kek",
            Title = "kek",
            ImageUrl = "kek"
        };
        
        
        int id = _recipeRepository.Insert(newRecipe);
        
        Recipe returnedRecipe = _recipeRepository.GetById(id);
        DeleteRecipe(id);
        
        
        Assert.Equal(newRecipe,returnedRecipe);
    }
    
    [Fact]
    public async Task InsertAsync_ReturnTrue()
    {
        Recipe newRecipe = new Recipe()
        {
            Url = "kek1",
            Title = "kek1",
            ImageUrl = "kek1"
        };
        
        
        int id = await _recipeRepository.InsertAsync(newRecipe);
        Recipe returnedRecipe = _recipeRepository.GetById(id);
        DeleteRecipe(id);
        
        
        Assert.Equal(newRecipe,returnedRecipe);
    }

    [Fact]
    public void DeleteById_ReturnsTrue()
    {
        Recipe newRecipe = new Recipe()
        {
            Url = "kek",
            Title = "kek",
            ImageUrl = "kek"
        };
        int id = InsertRecipeToTable(newRecipe);
        
        
        _recipeRepository.Delete(id);
        Recipe? recipe = _recipeRepository.GetById(id);
        
        
        Assert.Null(recipe);
    }
    
    [Fact]
    public async Task DeleteByIdAsync_ReturnsTrue()
    {
        Recipe newRecipe = new Recipe()
        {
            Url = "kek",
            Title = "kek",
            ImageUrl = "kek"
        };
        int id = InsertRecipeToTable(newRecipe);
        
        
        await _recipeRepository.DeleteAsync(id);
        Recipe? recipe = _recipeRepository.GetById(id);
        
        
        Assert.Null(recipe);
    }
    
    [Fact]
    public void DeleteByUrl_ReturnsTrue()
    {
        Recipe newRecipe = new Recipe()
        {
            Url = "kek",
            Title = "kek",
            ImageUrl = "kek"
        };
        int id = InsertRecipeToTable(newRecipe);
        
        
        _recipeRepository.Delete("kek");
        Recipe? recipe = _recipeRepository.GetById(id);
        
        
        Assert.Null(recipe);
    }

    [Fact]
    public void Update_DataExists_ReturnTrue()
    {
        Recipe recipe = new Recipe()
        {
            Url = "kek",
            Title = "kek",
            ImageUrl = "kek"
        };
        int id = InsertRecipeToTable(recipe);
        Recipe newRecipe = new Recipe()
        {
            RecipeId = id,
            Url = "kek2",
            Title = "kek2",
            ImageUrl = "kek2"
        };
        
        
        _recipeRepository.Update(newRecipe);
        Recipe updatedRecipe = _recipeRepository.GetById(id);
        DeleteRecipe(id);
        
        Assert.Equal(newRecipe, updatedRecipe);
    }
    
    [Fact]
    public async Task UpdateAsync_DataExists_ReturnTrue()
    {
        Recipe recipe = new Recipe()
        {
            Url = "kek",
            Title = "kek",
            ImageUrl = "kek"
        };
        int id = InsertRecipeToTable(recipe);
        Recipe newRecipe = new Recipe()
        {
            RecipeId = id,
            Url = "kek2",
            Title = "kek2",
            ImageUrl = "kek2"
        };
        
        
        await _recipeRepository.UpdateAsync(newRecipe);
        Recipe updatedRecipe = _recipeRepository.GetById(id);
        DeleteRecipe(id);
        
        Assert.Equal(newRecipe, updatedRecipe);
    }

    [Fact]
    public void GetRecipeIngredientsById_DataExists_ReturnTrue()
    {
        
    }
    
    [Fact]
    public async Task GetRecipeIngredientsByIdAsync_DataExists_ReturnTrue()
    {
        
    }
    
    [Fact]
    public void GetRecipeIngredientsByTitle_DataExists_ReturnTrue()
    {
        
    }
    
    [Fact]
    public async Task GetRecipeIngredientsByTitleAsync_DataExists_ReturnTrue()
    {
        
    }
    
    [Fact]
    public void GetRecipeIngredientsByUrl_DataExists_ReturnTrue()
    {
        
    }
    
    [Fact]
    public async Task GetRecipeIngredientsByUrlAsync_DataExists_ReturnTrue()
    {
        
    }
    
    [Fact]
    public void InsertReferencesForIngredients_DataExists_ReturnTrue()
    {
        
    }
    
    [Fact]
    public async Task InsertReferencesForIngredientsAsync_DataExists_ReturnTrue()
    {
        
    }
}