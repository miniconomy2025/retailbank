namespace RetailBank.Models.Dtos;

public record AccountTransfer(
    string BalancePending,
    string BalancePosted
);
