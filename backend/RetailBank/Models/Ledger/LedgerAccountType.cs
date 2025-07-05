using TigerBeetle;

namespace RetailBank.Models.Ledger;

/// <summary>
/// TigerBeetle account code for types of internal accounts
/// </summary>
public enum LedgerAccountType
{
    Internal = 1000,
    Transactional = 2000,
    Loan = 3000,
}

public static class LedgerAccountCodeExt
{
    public static AccountFlags ToAccountFlags(this LedgerAccountType code)
    {
        switch (code)
        {
            case LedgerAccountType.Transactional:
                return AccountFlags.DebitsMustNotExceedCredits;
            case LedgerAccountType.Loan:
                return AccountFlags.CreditsMustNotExceedDebits;
            default:
                return AccountFlags.None;
        }
    }
}
