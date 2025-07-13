using System.ComponentModel.DataAnnotations;
using RetailBank.Models.Ledger;
using RetailBank.Validation;

namespace RetailBank.Models.Dtos;

public record AccountDto(
    [property: Required]
    [property: Length(4, 12)]
    [property: RegularExpression(ValidationConstants.AccountNumber)]
    string Id,
    [property: Required]
    LedgerAccountType AccountType,
    [property: Required]
    [property: Range(0, ValidationConstants.UInt128Max)]
    UInt128 DebitsPending,
    [property: Required]
    [property: Range(0, ValidationConstants.UInt128Max)]
    UInt128 DebitsPosted,
    [property: Required]
    [property: Range(0, ValidationConstants.UInt128Max)]
    UInt128 CreditsPending,
    [property: Required]
    [property: Range(0, ValidationConstants.UInt128Max)]
    UInt128 CreditsPosted,
    [property: Required]
    [property: Range(0, ValidationConstants.UInt128Max)]
    Int128 BalancePending,
    [property: Required]
    [property: Range(0, ValidationConstants.UInt128Max)]
    Int128 BalancePosted,
    [property: Required]
    bool Closed,
    [property: Required]
    [property: Range(0, ulong.MaxValue)]
    ulong CreatedAt
)
{
    public AccountDto(LedgerAccount account)
        : this(
            account.Id.ToString(),
            account.AccountType,
            account.DebitsPending,
            account.DebitsPosted,
            account.CreditsPending,
            account.CreditsPosted,
            account.BalancePending,
            account.BalancePosted,
            account.Closed,
            account.Timestamp
        )
    { }
}
