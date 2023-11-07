namespace RecipeFetcherService.DbManagement.Interfaces;

public interface IRepository<T>
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    int Insert(T item);
    void Update(T item);
    void Delete(int id);
}