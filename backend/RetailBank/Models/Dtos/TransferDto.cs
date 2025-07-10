using RetailBank.Extensions;
using RetailBank.Models.Ledger;

namespace RetailBank.Models.Dtos;

public record TransferDto(
    string TransferId,
    TransferType TransferType,
    string DebitAccountId,
    string CreditAccountId,
    UInt128 Amount,
    string? ParentId,
    ulong Timestamp,
    ulong Reference
)
{
    public TransferDto(LedgerTransfer transfer)
        : this(
            transfer.Id.ToHex(),
            transfer.TransferType,
            transfer.DebitAccountId.ToString(),
            transfer.CreditAccountId.ToString(),
            transfer.Amount,
            transfer.ParentId.HasValue ? transfer.ParentId.Value.ToHex() : null,
            transfer.Timestamp,
            transfer.Reference
        )
    { }
}
