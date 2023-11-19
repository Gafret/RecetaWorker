using HtmlAgilityPack;
using Npgsql;
using RecipeFetcherService.DbManagement.DataClasses;
using RecipeFetcherService.DbManagement.Interfaces;
using RecipeFetcherService.Scraper.Interfaces;
using RecipeFetcherService.Scraper.PatternDeserialization;

namespace RecipeFetcherService.Scraper;

public class Parser
{
    // TODO: refactor
    private IRecipeRepository<Recipe, Ingredient> _recipeRepo;
    private IIngredientRepository<Ingredient, Recipe> _ingredientRepo;
    private IPatternReader<WebsitePatternRecord> _reader;
    
    public Parser(
        IRecipeRepository<Recipe, Ingredient> recipeRepo, 
        IIngredientRepository<Ingredient, Recipe> ingredientRepo, 
        IPatternReader<WebsitePatternRecord> reader
        )
    {
        _recipeRepo = recipeRepo;
        _ingredientRepo = ingredientRepo;
        _reader = reader;
    }

    public IEnumerable<WebsitePatternRecord> GetPatternsForRecipes(string pathToPatterns)
    {
        var patterns = _reader.ReadPatterns(pathToPatterns);
        return patterns;
    }

    public async Task<HtmlDocument> GetPage(string url)
    {
        return await WebCrawler.GetHtmlDocument(url);
    }

    public List<HtmlNode>? GetPageAttrs(HtmlDocument page, ITagPattern pattern)
    {
        return WebCrawler.FindTags(page, pattern);
    }

    public List<Ingredient> ToIngredients(List<HtmlNode> ingredientElements)
    {
        List<Ingredient> ingredients = new List<Ingredient>();
        foreach (var ingredientElement in ingredientElements)
        {
            Ingredient ingredient = new Ingredient()
            {
                Name = ingredientElement.InnerText,
            };
        }

        return ingredients;
    }

    public IEnumerable<int> AddIngredientsToDb(IEnumerable<Ingredient> ingredients)
    {
        HashSet<int> ingredientIds = new HashSet<int>();
        int id = 0;
        foreach (var ingredient in ingredients)
        {
            try
            {
                id = _ingredientRepo.Insert(ingredient);
            }
            catch (PostgresException exception)
            {
                if (exception.ConstraintName == "disallow_duplicate_ingrs")
                {
                    id = _ingredientRepo.GetByName(ingredient.Name).IngredientId;
                }
            }

            ingredientIds.Add(id);
        }

        return ingredientIds;
    }

    public int AddRecipeToDb(Recipe recipe)
    {
        int id = 0;
        try
        {
            id = _recipeRepo.Insert(recipe);
        }
        catch
        {
            id = _recipeRepo.GetByTitle(recipe.Title).RecipeId;
        }

        return id;
    }

    private string GetTitle(HtmlNode titleElement)
    {
        return titleElement.InnerText.Trim();
    }

    private string GetImageUrl(HtmlNode imageElement)
    {
        return imageElement.GetAttributeValue("src", null);
    }

    private (List<HtmlNode>, HtmlNode, HtmlNode) GetRecipeAttrs(HtmlDocument recipePage, WebsitePatternRecord pattern)
    {
        var ingredientElements = GetPageAttrs(recipePage, pattern.IngredientsPattern);
        var titleElement = GetPageAttrs(recipePage, pattern.TitlePattern).FirstOrDefault();
        var imageUrlElement = GetPageAttrs(recipePage, pattern.ImagePattern).FirstOrDefault();

        return (ingredientElements, titleElement, imageUrlElement);
    }

    private void AddEntryToDb(Recipe recipe, IEnumerable<Ingredient> ingredients)
    {
        int recipeId = AddRecipeToDb(recipe);
        IEnumerable<int> ingredientIds = AddIngredientsToDb(ingredients);

        _recipeRepo.InsertReferencesForIngredients(recipeId, ingredientIds);
    }
    
    private async Task ParseRecipe(string recipeUrl, WebsitePatternRecord pattern)
    {
        if (recipeUrl is null) return;
                
        HtmlDocument recipePage = await GetPage(recipeUrl);
        var (ingredientElements, titleElement, imageUrlElement) = GetRecipeAttrs(recipePage, pattern);
        

        List<Ingredient> ingredients = ToIngredients(ingredientElements);
        string title = GetTitle(titleElement);
        string imageUrl = GetImageUrl(imageUrlElement);

        Recipe recipe = new Recipe()
        {
            Title = title,
            ImageUrl = imageUrl,
            Url = recipeUrl,
        };

        AddEntryToDb(recipe, ingredients);
    }
    
    private async Task ParseWebsite(WebsitePatternRecord pattern)
    {
        HtmlDocument mainPage = await GetPage(pattern.Website);
        List<HtmlNode>? recipeLinks = GetPageAttrs(mainPage, pattern.LinkPattern);
            
        if (recipeLinks is null) return;

        foreach (var recipeLink in recipeLinks)
        {
            string recipeUrl = recipeLink.GetAttributeValue("href", null);

            ParseRecipe(recipeUrl, pattern);
        }
    }

    public async Task Parse(string pathToPatterns)
    {
        var patterns = GetPatternsForRecipes(pathToPatterns);
        foreach (var pattern in patterns)
        {
            ParseWebsite(pattern);
        }
    }
}