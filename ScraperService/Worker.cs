using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using RecipeFetcherService.DbManagement.DataClasses;
using RecipeFetcherService.DbManagement.Interfaces;
using RecipeFetcherService.DbManagement.Repositories;
using RecipeFetcherService.Scraper;
using RecipeFetcherService.Scraper.Abstract;
using RecipeFetcherService.Scraper.PatternDeserialization;

namespace RecipeFetcherService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IRecipeRepository<Recipe, Ingredient> _recipeRepository;
    private readonly IIngredientRepository<Ingredient, Recipe> _ingredientRepository;
    
    public Worker(ILogger<Worker> logger, IOptions<DbOptions> dbOptions)
    {
        IDbConnection connection = new NpgsqlConnection(dbOptions.Value.PostgreSqlTest);
        
        _logger = logger;
        _recipeRepository = new RecipeRepository(connection);
        _ingredientRepository = new IngredientRepository(connection);
        
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ParserBase parser = new Parser(_recipeRepository, _ingredientRepository, new TagPatternReader());
        await parser.Parse("./tag_pattern.json");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}