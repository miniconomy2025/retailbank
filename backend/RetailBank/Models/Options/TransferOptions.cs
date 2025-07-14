namespace RetailBank.Models.Options;

public record TransferOptions
{
    public const string Section = "Transfer";

    public required decimal TransferFeePercent { get; init; }
    public required decimal DepositFeePercent { get; init; }
}
