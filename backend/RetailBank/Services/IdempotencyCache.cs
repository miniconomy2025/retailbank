using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using RetailBank.Exceptions;

namespace RetailBank.Services;

public class IdempotencyCache(IMemoryCache cache) : IIdempotencyCache
{
    /// <summary>
    /// Returns true if a key is already present, and holds it for some length of time.
    /// </summary>
    public bool Insert<T>(T obj)
    {
        var key = JsonSerializer.Serialize(obj);
        var present = cache.Get(key) != null;
        cache.Set(key, true, DateTimeOffset.Now.AddHours(1));
        return present;
    }

    public void InsertAndThrow<T>(T obj)
    {
        if (Insert(obj))
            throw new IdempotencyException();
    }

    public void Clear<T>(T obj)
    {
        var key = JsonSerializer.Serialize(obj);
        cache.Remove(key);
    }
}
