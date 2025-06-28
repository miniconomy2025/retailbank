namespace RetailBank.Models;

/// <summary>
/// TigerBeetle account code for types of internal accounts
/// </summary>
public enum LedgerAccountCode
{
    Bank = 1000,
    OwnersEquityAccount = 1001,
    Savings = 2000,
    Loan = 3000,
    InterestIncomeAccount = 3001,
    LoanControlAccount = 3002,
}
