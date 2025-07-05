namespace RetailBank.Models.Dtos;

public record CreateTransferRequest(
    string From,
    string To,
    UInt128 AmountCents,
    ulong? Reference
);
