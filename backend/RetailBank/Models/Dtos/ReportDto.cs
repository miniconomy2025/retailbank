using System.ComponentModel.DataAnnotations;
using RetailBank.Validation;

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
    [property: Range(0, ValidationConstants.UInt128Max)]
    UInt128 TotalMoney
);    
