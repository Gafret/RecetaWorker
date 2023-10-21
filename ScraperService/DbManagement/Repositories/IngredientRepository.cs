using System.Data;
using Dapper;
using Npgsql;
using RecipeFetcherService.DbManagement.DataClasses;

namespace RecipeFetcherService.DbManagement.Repositories;

public class IngredientRepository : IRepository<Ingredient>
{
    private readonly IDbConnection _dbConnection;
    public IngredientRepository(IDbConnection  connection)
    {
        _dbConnection = connection;
    }
    public IEnumerable<Ingredient> GetAll()
    {
        IEnumerable<Ingredient> ingredients = _dbConnection.Query<Ingredient>("SELECT ingredient_id, name FROM ingredients");
        return ingredients;
    }

    public Ingredient GetById(int id)
    {
        var @params = new { Id = id };
        Ingredient ingredient = _dbConnection.Query<Ingredient>(
            "SELECT ingredient_id, name FROM ingredients WHERE ingredient_id = @Id", 
            @params).Single();
        
        return ingredient;
    }

    public void Insert(Ingredient item)
    {
        _dbConnection.Query(
            "INSERT INTO ingredients(name) VALUES (@Name)", item);
    }

    public void Update(Ingredient item)
    {
        _dbConnection.Query(
            "UPDATE ingredients SET name = @Name WHERE ingredient_id = @IngredientId", item);
    }

    public void Delete(int id)
    {
        var @params = new { Id = id };
        _dbConnection.Query(
            "DELETE FROM ingredients WHERE ingredient_id = @IngredientId", @params);
    }

    public void Save()
    {
        throw new NotImplementedException();
    }
}