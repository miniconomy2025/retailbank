namespace RetailBank.Validation;

public static class ValidationConstants
{
    public const string AccountNumber = "^([0-9]{4})|([0-9]{12,13})$";
    public const string Base10 = "^[0-9]+$";
    public const string Hex = "^[0-9A-F]+$";
    public const double UInt128Max = 340282366920938463463374607431768211455.0;
}
