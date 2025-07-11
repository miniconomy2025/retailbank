using RetailBank.Models.Ledger;

namespace RetailBank.Models.Options;

public class InterbankTransferOptions
{
    public const string Section = "InterbankNotification";

    public required uint RetryCount { get; init; } = 3;
    public required uint DelaySeconds { get; init; } = 15;
    public required string ClientCertificatePath { get; init; }
    public required string ClientCertificateKeyPath { get; init; }
    public required ulong LoanAmountCents { get; init; } = 1_000_000_00;
    public required IDictionary<Bank, InterbankTransferBankDetails> Banks { get; init; }
}
