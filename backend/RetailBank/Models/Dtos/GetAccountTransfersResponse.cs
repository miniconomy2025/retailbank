namespace RetailBank.Models.Dtos;

public record GetAccountResponse(
    IEnumerable<AccountTransfer> Transfers
);
