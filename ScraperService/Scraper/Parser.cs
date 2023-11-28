using HtmlAgilityPack;
using Npgsql;
using RecipeFetcherService.DbManagement.DataClasses;
using RecipeFetcherService.DbManagement.Interfaces;
using RecipeFetcherService.Scraper.Abstract;
using RecipeFetcherService.Scraper.Interfaces;
using RecipeFetcherService.Scraper.PatternDeserialization;

namespace RecipeFetcherService.Scraper;

public class Parser : ParserBase
{
    public Parser(
        IRecipeRepository<Recipe, Ingredient> recipeRepo,
        IIngredientRepository<Ingredient, Recipe> ingredientRepo,
        IPatternReader<WebsitePatternRecord> reader
    ) : base(recipeRepo, ingredientRepo, reader){}

    
    // helpers
    private IEnumerable<WebsitePatternRecord> GetPatternsForRecipes(string pathToPatterns)
    {
        var patterns = _reader.ReadPatterns(pathToPatterns);
        return patterns;
    }

    private async Task<HtmlDocument> GetPage(string url)
    {
        return await WebCrawler.GetHtmlDocument(url);
    }

    private List<HtmlNode>? GetPageAttrs(HtmlDocument page, ITagPattern pattern)
    {
        return WebCrawler.FindTags(page, pattern);
    }
    private string GetText(HtmlNode titleElement)
    {
        return titleElement.InnerText.Trim();
    }

    private string GetElementAttr(HtmlNode element, string attrName)
    {
        return element.GetAttributeValue(attrName, null);
    }

    private (List<HtmlNode>?, HtmlNode?, HtmlNode?) GetRecipeAttrs(HtmlDocument recipePage, WebsitePatternRecord pattern)
    {
        var ingredientElements = GetPageAttrs(recipePage, pattern.IngredientsPattern);
        var titleElement = GetPageAttrs(recipePage, pattern.TitlePattern)?.FirstOrDefault();
        var imageUrlElement = GetPageAttrs(recipePage, pattern.ImagePattern)?.FirstOrDefault();

        return (ingredientElements, titleElement, imageUrlElement);
    }

    private List<Ingredient> MapToIngredients(List<HtmlNode> ingredientElements)
    {
        List<Ingredient> ingredients = new List<Ingredient>();
        foreach (var ingredientElement in ingredientElements)
        {
            Ingredient ingredient = new Ingredient()
            {
                Name = ingredientElement.InnerText,
            };
            ingredients.Add(ingredient);
        }

        return ingredients;
    }

    
    
    // db work
    private IEnumerable<int> AddIngredientsToDb(IEnumerable<Ingredient> ingredients)
    {
        HashSet<int> ingredientIds = new HashSet<int>();
        int id;
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
                    // not null because triggers only where duplicates appear
                    id = _ingredientRepo.GetByName(ingredient.Name).IngredientId;
                }
                else
                {
                    continue;
                }
            }

            ingredientIds.Add(id);
        }

        return ingredientIds;
    }

    private int AddRecipeToDb(Recipe recipe)
    {
        int id;
        try
        {
            id = _recipeRepo.Insert(recipe);
        }
        catch (PostgresException exception)
        {
            if (exception.ConstraintName == "disallow_duplicate_urls" ||
                exception.ConstraintName == "recipes_title_key")
            {
                // not null because triggers only if there is duplication error
                id = _recipeRepo.GetByTitle(recipe.Title).RecipeId;
            }
            else throw;
        }

        return id;
    }
    
    private void AddFullEntryToDb(Recipe recipe, IEnumerable<Ingredient> ingredients)
    {
        int recipeId = AddRecipeToDb(recipe);
        IEnumerable<int> ingredientIds = AddIngredientsToDb(ingredients);

        try
        {
            _recipeRepo.InsertReferencesForIngredients(recipeId, ingredientIds);
        }
        catch(PostgresException exc)
        {
            if (exc.ConstraintName == "recipe_ingredients_pkey")
                return;
            else
                throw;
        }
    }
    
    
    
    // main methods
    private async Task ParseRecipe(string recipeUrl, WebsitePatternRecord pattern)
    {
        HtmlDocument recipePage = await GetPage(recipeUrl);
        
        var (ingredientElements, 
            titleElement, 
            imageUrlElement) = GetRecipeAttrs(recipePage, pattern);

        if (ingredientElements is null ||
            titleElement is null) 
            return;

        List<Ingredient> ingredients = MapToIngredients(ingredientElements);
        string title = GetText(titleElement);
        string? imageUrl = null; // not all recipes have an image to them
        if(imageUrlElement is not null) 
            imageUrl = GetElementAttr(imageUrlElement, "src");

        Recipe recipe = new Recipe()
        {
            Title = title,
            ImageUrl = imageUrl,
            Url = recipeUrl,
        };

        AddFullEntryToDb(recipe, ingredients);
    }
    
    private async Task ParseCategory(string categoryPageUrl, WebsitePatternRecord pattern)
    {
        HtmlDocument categoryPage = await GetPage(categoryPageUrl);
        List<HtmlNode>? recipeLinks = GetPageAttrs(categoryPage, pattern.LinkPattern);
            
        if (recipeLinks is null) 
            return;
        
        foreach (var recipeLink in recipeLinks)
        {
            string recipeUrl = recipeLink.GetAttributeValue("href", null);
            if (recipeUrl is null) continue;
            await ParseRecipe(recipeUrl, pattern);
        }
    }
    
    private async Task ParseWebsite(WebsitePatternRecord pattern)
    {
        HtmlDocument mainPage = await GetPage(pattern.Website);
        List<HtmlNode>? categories = GetPageAttrs(mainPage, pattern.RecipeCategoryPattern);

        if (categories is null)
        {
            // In case if there is no dedicated page with all recipe categories
            // on the website, get only the recipes from the root page (pattern.Website)
            await ParseCategory(pattern.Website, pattern);
        }
        else
        {
            foreach (var category in categories)
            {
                string categoryPageUrl = category.GetAttributeValue("href", null);
                if(categoryPageUrl is null) continue;
                await ParseCategory(categoryPageUrl, pattern);
            }
        }
    }

    
    public override async Task Parse(string pathToPatterns)
    {
        var patterns = GetPatternsForRecipes(pathToPatterns);
        
        foreach (var pattern in patterns)
        {
            await ParseWebsite(pattern);
        }
    }
}