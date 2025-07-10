namespace RetailBank.Exceptions;

public class AccountNotFoundException : UserException
{
    public AccountNotFoundException(UInt128 accountId) : base(
        StatusCodes.Status404NotFound,
        "Account Not Found",
        $"Could not find account {accountId}."
    )
    { }
}
