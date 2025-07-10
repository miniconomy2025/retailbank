namespace RetailBank.Services;

public interface IIdempotencyCache
{
    void InsertAndThrow<T>(T key);
    bool Insert<T>(T key);
    void Clear<T>(T key);
}
