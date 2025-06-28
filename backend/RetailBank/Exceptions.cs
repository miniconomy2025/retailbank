public class TigerBeetleResultException<T> : Exception
{
    public T? ErrorCode { get; }
    public TigerBeetleResultException(T errorCode) : base($"{typeof(T).Name} Exception. Error code: {errorCode}") { ErrorCode = errorCode; }
}

public class ExternalTransferFailedException : Exception
{
   public ExternalTransferFailedException() : base("Failed to transfer to commercial account") { } 
}

public class AccountNotFoundException : Exception
{
    public AccountNotFoundException(UInt128 accountId) : base($"Failed to retrieve account: {accountId}") { }
}

public class InvalidAccountException : Exception
{
    public InvalidAccountException() : base($"One or more of the provided accounts is not a client account."){}
}
