namespace RetailBank.Models;

/// <summary>
/// TigerBeetle account IDs for types of internal accounts
/// </summary>
public enum LedgerAccountId
{
    // must be same as BankCode.Retail
    Bank = 1000,
    OwnersEquity = 1001,
    InterestIncome = 1002,
    LoanControl = 1003,
    BadDebts = 1004,
}
