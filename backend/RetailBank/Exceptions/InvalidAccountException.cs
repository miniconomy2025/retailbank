using RetailBank.Models.Ledger;

namespace RetailBank.Exceptions;

public class InvalidAccountException : UserException
{
    public InvalidAccountException(LedgerAccountType got, LedgerAccountType expected) : base(StatusCodes.Status400BadRequest, "Invalid Account", $"Expected {expected.ToString().ToLower()} account, but got {got.ToString().ToLower()} account.") { }
}
