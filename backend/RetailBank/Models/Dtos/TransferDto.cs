using System.ComponentModel.DataAnnotations;
using RetailBank.Extensions;
using RetailBank.Models.Ledger;
using RetailBank.Validation;

namespace RetailBank.Models.Dtos;

public record TransferDto(
    [property: Required]
    [property: Length(32, 32)]
    [property: RegularExpression(ValidationConstants.Hex)]
    string TransferId,
    [property: Required]
    TransferType TransferType,
    [property: Required]
    [property: Length(4, 13)]
    [property: RegularExpression(ValidationConstants.AccountNumber)]
    string DebitAccountId,
    [property: Required]
    [property: Length(4, 13)]
    [property: RegularExpression(ValidationConstants.AccountNumber)]
    string CreditAccountId,
    [property: Required]
    [property: Range(0, ValidationConstants.UInt128Max)]
    UInt128 Amount,
    [property: Length(32, 32)]
    [property: RegularExpression(ValidationConstants.Hex)]
    string? ParentId,
    [property: Required]
    [property: Range(1, ulong.MaxValue)]
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
            transfer.Reference
        )
    { }
}
