using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Models.Ledger;

public record LedgerAccount(
    UInt128 Id,
    LedgerAccountType AccountType,
    DebitOrder? DebitOrder = null,
    bool Closed = false,
    UInt128 DebitsPending = default,
    UInt128 DebitsPosted = default,
    UInt128 CreditsPending = default,
    UInt128 CreditsPosted = default,
    ulong Cursor = 0
)
{
    public Int128 BalancePending => (Int128)DebitsPending - (Int128)CreditsPending;
    public Int128 BalancePosted => (Int128)DebitsPosted - (Int128)CreditsPosted;

    public LedgerAccount(Account account)
        : this(
            account.Id,
            (LedgerAccountType)account.Code,
            account.UserData128 > 0
              ? new DebitOrder(account.UserData128, account.UserData64)
              : null,
            (account.Flags & AccountFlags.Closed) > 0,
            account.DebitsPending,
            account.DebitsPosted,
            account.CreditsPending,
            account.CreditsPosted,
            account.Timestamp
        )
    { }

    public Account ToAccount()
    {
        var closedFlag = Closed ? AccountFlags.Closed : AccountFlags.None;

        return new Account
        {
            Id = Id,
            Code = (ushort)AccountType,
            Ledger = LedgerRepository.LedgerId,
            Timestamp = Cursor,
            UserData128 = DebitOrder?.DebitAccountId ?? 0,
            UserData64 = DebitOrder?.Amount ?? 0,
            Flags = AccountType.ToAccountFlags() | closedFlag,
        };
    }
}
