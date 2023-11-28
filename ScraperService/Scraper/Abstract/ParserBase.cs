using RecipeFetcherService.DbManagement.DataClasses;
using RecipeFetcherService.DbManagement.Interfaces;
using RecipeFetcherService.Scraper.Interfaces;
using RecipeFetcherService.Scraper.PatternDeserialization;

namespace RecipeFetcherService.Scraper.Abstract;

public abstract class ParserBase
{
    protected IRecipeRepository<Recipe, Ingredient> _recipeRepo;
    protected IIngredientRepository<Ingredient, Recipe> _ingredientRepo;
    protected IPatternReader<WebsitePatternRecord> _reader; 
    
    protected ParserBase(
        IRecipeRepository<Recipe, Ingredient> recipeRepo, 
        IIngredientRepository<Ingredient, Recipe> ingredientRepo, 
        IPatternReader<WebsitePatternRecord> reader
    )
    {
        _recipeRepo = recipeRepo;
        _ingredientRepo = ingredientRepo;
        _reader = reader;
    }
    public abstract Task Parse(string pathToPatterns);
}