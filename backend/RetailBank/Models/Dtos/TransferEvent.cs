using TigerBeetle;

namespace RetailBank.Models.Dtos;

public record TransferEvent(
    string TransactionId,
    ulong DebitAccountNumber,
    ulong CreditAccountNumber,
    UInt128 Amount,
    string? PendingId,
    ulong Timestamp,
    TransferEventType EventType
)
{
    public TransferEvent(Transfer transfer)
        : this(
            transfer.Id.ToString("X"),
            (ulong)transfer.DebitAccountId,
            (ulong)transfer.CreditAccountId,
            transfer.Amount,
            transfer.PendingId > 0 ? transfer.PendingId.ToString("X") : null,
            transfer.Timestamp,
            MapFlags(transfer.Flags)
        )
    { }

    private static TransferEventType MapFlags(TransferFlags flags)
    {
        if ((flags & TransferFlags.Pending) > 0)
            return TransferEventType.StartTransfer;
        else if ((flags & TransferFlags.PostPendingTransfer) > 0)
            return TransferEventType.CompleteTransfer;
        else if ((flags & TransferFlags.VoidPendingTransfer) > 0)
            return TransferEventType.CancelTransfer;
        
        return TransferEventType.Transfer;
    }
}
