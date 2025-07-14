using System.ComponentModel.DataAnnotations;

namespace RetailBank.Models.Dtos;

public record Report(
    [property: Required]
    [property: Range(0, uint.MaxValue)]
    uint TransactionalAccounts,
    [property: Required]
    [property: Range(0, uint.MaxValue)]
    uint LoanAccounts,
    [property: Required]
    Int128 BankBalance,
    [property: Required]
    UInt128 RecentVolume,
    [property: Required]
    [property: RegularExpression("^\\d+\\/\\d+\\/\\d+\\ \\d+:\\d+:\\d+$")]
    string Timestamp
);
