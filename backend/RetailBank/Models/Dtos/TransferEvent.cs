namespace RetailBank.Models.Dtos;

public record TransferEvent(
    string TransactionId,
    ulong DebitAccountNumber,
    ulong CreditAccountNumber,
    UInt128 Amount,
    string? PendingId,
    ulong Timestamp,
    TransferEventType EventType
);
