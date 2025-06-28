namespace RetailBank.Models.Dtos;

public record GetAccountBalanceResponse(
    string BalancePending,
    string BalancePosted
);
