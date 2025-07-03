using RetailBank.Services;
using TigerBeetle;

namespace RetailBank.Models.Dtos;

public record TransferEvent(
    string TransactionId,
    UInt128 DebitAccountNumber,
    UInt128 CreditAccountNumber,
    UInt128 Amount,
    string? PendingId,
    ulong Timestamp,
    TransferEventType EventType
)
{
    public TransferEvent(Transfer transfer, TransferEventType eventType)
        : this(
            transfer.Id.ToString("X"),
            transfer.DebitAccountId,
            // If interbank transfer, credit account is in userData128
            Enum.IsDefined((BankId)(ulong)transfer.CreditAccountId) && transfer.CreditAccountId != (ulong)BankId.Retail
              ? transfer.UserData128 : transfer.CreditAccountId,
            transfer.Amount,
            transfer.PendingId > 0 ? transfer.PendingId.ToString("X") : null,
            transfer.Timestamp,
            eventType
        )
    { }

    public static async Task<TransferEventType> MapEventType(Transfer transfer, ITransferService service)
    {
        var flags = transfer.Flags;

        if ((flags & TransferFlags.ClosingCredit) > 0)
            return TransferEventType.ClosingCredit;
        
        if ((flags & TransferFlags.ClosingDebit) > 0)
            return TransferEventType.ClosingDebit;

        if ((flags & TransferFlags.Pending) > 0)
            return TransferEventType.StartTransfer;
        
        if ((flags & TransferFlags.PostPendingTransfer) > 0)
            return TransferEventType.CompleteTransfer;
        
        if ((flags & TransferFlags.VoidPendingTransfer) > 0)
        {
            var pendingTransfer = await service.GetTransfer(transfer.PendingId);

            if (pendingTransfer?.EventType == TransferEventType.ClosingCredit)
                return TransferEventType.ReopenCredit;
            if (pendingTransfer?.EventType == TransferEventType.ClosingCredit)
                return TransferEventType.ReopenDebit;

            return TransferEventType.CancelTransfer;
        }

        return TransferEventType.Transfer;
    }

}
