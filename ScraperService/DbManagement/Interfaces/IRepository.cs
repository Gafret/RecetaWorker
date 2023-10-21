namespace RecipeFetcherService.DbManagement;

public interface IRepository<T>
{
    IEnumerable<T> GetAll();
    T GetById(int id);
    void Insert(T item);
    void Update(T item);
    void Delete(int id);
    void Save();
}