namespace RetailBank.Validation;

public static class ValidationConstants
{
    public const string TransferFromAccountNumber = TransactionalAccountNumber;
    public const string TransferToAccountNumber = "^[0-9]{12,13}$";
    public const string TransactionalAccountNumber = "^[0-9]{12}$";
}
