namespace RetailBank.Models.Options;

public record TransferOptions
{
    public const string Section = "Transfer";

    public decimal TransferFeePercent { get; init; } = 2.0m;
    public decimal DepositFeePercent { get; init; } = 0.25m;
}
