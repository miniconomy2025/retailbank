using RetailBank.Extensions;
using RetailBank.Models.Ledger;

namespace RetailBank.Models.Dtos;

public record TransferDto(
    string TransactionId,
    UInt128 DebitAccountNumber,
    UInt128 CreditAccountNumber,
    UInt128 Amount,
    string? PendingId,
    ulong Timestamp,
    TransferKind EventType,
    ulong? Reference
)
{
    public TransferDto(LedgerTransfer transfer)
        : this(
            transfer.Id.ToHex(),
            transfer.DebitAccountId,
            transfer.CreditAccountId,
            transfer.Amount,
            transfer.PendingId.HasValue ? transfer.PendingId.Value.ToHex() : null,
            transfer.Timestamp,
            transfer.Kind,
            transfer.Reference > 0 ? transfer.Reference : null
        )
    { }
}
