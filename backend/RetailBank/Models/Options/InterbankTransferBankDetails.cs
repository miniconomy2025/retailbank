namespace RetailBank.Models.Options;

public class InterbankTransferBankDetails
{
    public required string CreateAccountUrl { get; init; } = "";
    public required string GetAccountUrl { get; init; } = "";
    public required string GetAccountBalanceUrl { get; init; } = "";
    public required string IssueLoanUrl { get; init; } = "";
    public required string TransferUrl { get; init; } = "";
}
