namespace RetailBank.Models.Options;

public record LoanOptions
{
    public const string Section = "Loan";

    public decimal AnnualInterestRatePercentage { get; init; } = 10.0m;
    public uint LoanPeriodMonths { get; init; } = 60;
}
