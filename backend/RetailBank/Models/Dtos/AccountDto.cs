using TigerBeetle;

namespace RetailBank.Models.Dtos;

public record AccountDto(
    LedgerAccountCode AccountType,
    UInt128 DebitsPending,
    UInt128 DebitsPosted,
    UInt128 CreditsPending,
    UInt128 CreditsPosted
)
{
    public Int128 BalancePending => (Int128)CreditsPending - (Int128)DebitsPending;
    public Int128 BalancePosted => (Int128)CreditsPosted - (Int128)DebitsPosted;

    public AccountDto(Account account)
        : this(
            (LedgerAccountCode)account.Code,
            account.DebitsPending,
            account.DebitsPosted,
            account.CreditsPending,
            account.CreditsPosted
        )
    { }
}
