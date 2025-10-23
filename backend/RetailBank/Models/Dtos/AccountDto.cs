using System.ComponentModel.DataAnnotations;
using RetailBank.Models.Ledger;
using RetailBank.Services;
using RetailBank.Validation;
using TigerBeetle;

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
    DateTime CreatedAt
)
{
    public AccountDto(LedgerAccount account, SimulationControllerService simulation)
        : this(
            account.Id.ToString(),
            account.AccountType,
            new BalanceDto(account.DebitsPending, account.CreditsPending, account.BalancePending),
            new BalanceDto(account.DebitsPosted, account.CreditsPosted, account.BalancePosted),
            account.Closed,
            DateTimeOffset.FromUnixTimeMilliseconds((long)simulation.TimestampToSim(account.Cursor / 1000000)).UtcDateTime
        )
    { }
}
