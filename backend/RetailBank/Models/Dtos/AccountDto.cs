using RetailBank.Models.Ledger;

namespace RetailBank.Models.Dtos;

public record AccountDto(
    string Id,
    LedgerAccountCode AccountType,
    UInt128 DebitsPending,
    UInt128 DebitsPosted,
    UInt128 CreditsPending,
    UInt128 CreditsPosted,
    Int128 BalancePending,
    Int128 BalancePosted,
    bool Closed,
    ulong CreatedAt
)
{
    public AccountDto(LedgerAccount account)
        : this(
            account.Id.ToString(),
            account.AccountType,
            account.DebitsPending,
            account.DebitsPosted,
            account.CreditsPending,
            account.CreditsPosted,
            account.BalancePending,
            account.BalancePosted,
            account.Closed,
            account.Timestamp
        )
    { }
}
