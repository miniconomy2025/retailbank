using RetailBank.Extensions;
using RetailBank.Models.Ledger;

namespace RetailBank.Models.Dtos;

public record TransferDto(
    string TransferId,
    string DebitAccountId,
    string CreditAccountId,
    UInt128 Amount,
    string? ParentId,
    ulong Timestamp,
    TransferAction EventType,
    ulong? Reference
)
{
    public TransferDto(LedgerTransfer transfer)
        : this(
            transfer.Id.ToHex(),
            transfer.DebitAccountId.ToString(),
            transfer.CreditAccountId.ToString(),
            transfer.Amount,
            transfer.ParentId.HasValue ? transfer.ParentId.Value.ToHex() : null,
            transfer.Timestamp,
            transfer.Action,
            transfer.Reference > 0 ? transfer.Reference : null
        )
    { }
}
