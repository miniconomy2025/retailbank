namespace RetailBank.Models.Options;

public record LoanOptions
{
    public const string Section = "Loan";

    public required decimal AnnualInterestRatePercentage { get; init; } = 10.0m;
    public required uint LoanPeriodMonths { get; init; } = 60;
}
