using RetailBank.Models.Ledger;

namespace RetailBank.Models.Options;

public class InterbankTransferOptions
{
    public const string Section = "InterbankTransfer";

    public required uint RetryCount { get; init; } = 3;
    public required uint DelaySeconds { get; init; } = 10;
    public required string ClientCertificatePath { get; init; }
    public required string ClientCertificateKeyPath { get; init; }
    public required UInt128 LoanAmountCents { get; init; } = 10_000_000__00;
    public required Dictionary<Bank, InterbankTransferBankDetails> Banks { get; init; }
}
