namespace RetailBank.Validation;

public static class ValidationConstants
{
    public const string TransferFromAccountNumber = TransactionalAccountNumber;
    public const string TransferToAccountNumber = "^[0-9]{12,13}$";
    public const string TransactionalAccountNumber = "^[0-9]{12}$";
    public const string LoanAccountNumber = "^1000[0-9]{9}$";
    public const string AccountNumber = "^([0-9]{4})|([0-9]{12,13})$";
    public const string Hex = "^[0-9A-F]+$";
    public const double UInt128Max = 340282366920938463463374607431768211455.0;
}
