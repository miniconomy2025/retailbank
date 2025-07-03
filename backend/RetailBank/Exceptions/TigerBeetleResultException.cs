namespace RetailBank.Exceptions;

public class TigerBeetleResultException<T> : Exception
{
    public T? ErrorCode { get; }
    public TigerBeetleResultException(T errorCode) : base($"{typeof(T).Name} Error: {errorCode}")
    {
        ErrorCode = errorCode;
    }
}
