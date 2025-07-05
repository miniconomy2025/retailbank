using TigerBeetle;

namespace RetailBank.Models.Ledger;

public enum TransferAction
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
    public static TransferFlags ToTransferFlags(this TransferAction transferType)
    {
        return (TransferFlags)transferType;
    }

    public static TransferAction ToTransferKind(this TransferFlags flags)
    {
        return (TransferAction)flags;
    }
}
