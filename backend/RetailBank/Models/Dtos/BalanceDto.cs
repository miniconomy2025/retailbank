using RetailBank.Validation;
using System.ComponentModel.DataAnnotations;

namespace RetailBank.Models.Dtos;

public record BalanceDto(
    [property: Range(0, ValidationConstants.UInt128Max)]
    UInt128 Debits,
    [property: Required]
    [property: Range(0, ValidationConstants.UInt128Max)]
    UInt128 Credits,
    [property: Required]
    Int128 Balance
);
