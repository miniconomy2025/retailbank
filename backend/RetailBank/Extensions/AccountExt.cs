using TigerBeetle;

namespace RetailBank.Extensions;

public static class AccountExt
{
    public static Int128 BalancePending(this Account account)
    {
        return (Int128)account.DebitsPending - (Int128)account.CreditsPending;
    }

    public static Int128 BalancePosted(this Account account)
    {
        return (Int128)account.DebitsPosted - (Int128)account.CreditsPosted;
    }
}
