using TigerBeetle;

namespace RetailBank.Models.Ledger;

public record LedgerAccount(
    UInt128 Id,
    LedgerAccountCode AccountType,
    UInt128 DebitsPending,
    UInt128 DebitsPosted,
    UInt128 CreditsPending,
    UInt128 CreditsPosted,
    UInt128 LoanDebitAccount,
    ulong SalaryOrInstallment,
    bool Closed,
    ulong Timestamp
)
{
    public Int128 BalancePending => (Int128)DebitsPending - (Int128)CreditsPending;
    public Int128 BalancePosted => (Int128)DebitsPosted - (Int128)CreditsPosted;

    public LedgerAccount(Account account)
        : this(
            account.Id,
            (LedgerAccountCode)account.Code,
            account.DebitsPending,
            account.DebitsPosted,
            account.CreditsPending,
            account.CreditsPosted,
            account.UserData128,
            account.UserData64,
            (account.Flags & AccountFlags.Closed) > 0,
            account.Timestamp
        )
    { }
}
