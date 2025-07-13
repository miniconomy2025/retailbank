using System.ComponentModel.DataAnnotations;
using RetailBank.Models.Ledger;
using RetailBank.Validation;

namespace RetailBank.Models.Dtos;

public record AccountDto(
    [property: Required]
    [property: Length(4, 13)]
    [property: RegularExpression(ValidationConstants.AccountNumber)]
    string Id,
    [property: Required]
    LedgerAccountType AccountType,
    [property: Required]
    BalanceDto Pending,
    [property: Required]
    BalanceDto Posted,
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
            new BalanceDto(account.DebitsPending, account.CreditsPending, account.BalancePending),
            new BalanceDto(account.DebitsPosted, account.CreditsPosted, account.BalancePosted),
            account.Closed,
            account.Timestamp
        )
    { }
}
