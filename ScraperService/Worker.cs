using System.Data;
using Npgsql;
using RecipeFetcherService.DbManagement;
using RecipeFetcherService.DbManagement.DataClasses;
using RecipeFetcherService.DbManagement.Interfaces;
using RecipeFetcherService.DbManagement.Repositories;

namespace RecipeFetcherService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IRepository<Recipe> _recipeRepository;
    private readonly IRepository<Ingredient> _ingredientRepository;
    
    public Worker(ILogger<Worker> logger, IDbConnection connection)
    {
        _logger = logger;
        _recipeRepository = new RecipeRepository(connection);
        _ingredientRepository = new IngredientRepository(connection);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}