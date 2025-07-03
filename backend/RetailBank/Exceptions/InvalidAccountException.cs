using RetailBank.Models;

namespace RetailBank.Exceptions;

public class InvalidAccountException : UserException
{
    public InvalidAccountException(LedgerAccountCode got, LedgerAccountCode expected) : base(StatusCodes.Status400BadRequest, "Invalid Account", $"Expected {expected.ToString().ToLower()} account, but got {got.ToString().ToLower()} account.") { }
}
