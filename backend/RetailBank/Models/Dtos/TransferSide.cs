using TigerBeetle;

namespace RetailBank.Models.Dtos;

public enum TransferSide
{
    Debit = 1,
    Credit = 2,
    Any = 3,
}

public static class TransferSideExt
{
    public static AccountFilterFlags ToAccountFilterFlags(this TransferSide side)
    {
        return (AccountFilterFlags)side;
    }
}
