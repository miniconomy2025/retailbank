using TigerBeetle;

namespace RetailBank.Models.Ledger;

public enum TransferSide
{
    Debit = 1,
    Credit = 2,
}

public static class TransferSideExt
{
    public static AccountFilterFlags ToAccountFilterFlags(this TransferSide side)
    {
        return (AccountFilterFlags)side;
    }
}
