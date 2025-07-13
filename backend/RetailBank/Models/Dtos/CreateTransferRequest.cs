using System.ComponentModel.DataAnnotations;
using RetailBank.Validation;

namespace RetailBank.Models.Dtos;

public record CreateTransferRequest(
    [property: Required]
    [property: Length(12, 12)]
    [property: RegularExpression(ValidationConstants.TransferFromAccountNumber)]
    string From,
    [property: Required]
    [property: Length(12, 13)]
    [property: RegularExpression(ValidationConstants.TransferToAccountNumber)]
    string To,
    [property: Required]
    [property: Range(1, ValidationConstants.UInt128Max)]
    UInt128 AmountCents,
    [property: Required]
    [property: Range(1, ulong.MaxValue)]
    ulong Reference
);
