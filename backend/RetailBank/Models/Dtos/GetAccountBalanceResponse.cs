namespace RetailBank.Models.Dtos;

public record GetAccountBalanceResponse(
    Int128 BalancePending,
    Int128 BalancePosted
);
