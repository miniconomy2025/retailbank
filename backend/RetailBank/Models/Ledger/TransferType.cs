using TigerBeetle;

namespace RetailBank.Models.Ledger;

public enum TransferType
{
    Transfer = 0,
    StartTransfer = 2,
    CompleteTransfer = 4,
    CancelTransfer = 8,
    BalanceDebit = 0x10,
    BalanceCredit = 0x20,
    CloseDebit = 0x40 | StartTransfer,
    CloseCredit = 0x80 | StartTransfer,
}

public static class TransferActionExt
{
    private const TransferType TransferTypeMask =
        TransferType.Transfer | 
        TransferType.StartTransfer | 
        TransferType.CompleteTransfer | 
        TransferType.CancelTransfer | 
        TransferType.BalanceDebit | 
        TransferType.BalanceCredit | 
        TransferType.CloseDebit | 
        TransferType.CloseCredit;

    public static TransferFlags ToTransferFlags(this TransferType transferType)
    {
        return (TransferFlags)(transferType & TransferTypeMask);
    }

    public static TransferType ToTransferType(this TransferFlags flags)
    {
        return (TransferType)(ushort)flags & TransferTypeMask;
    }
}
