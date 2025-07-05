namespace RetailBank.Models.Ledger;

public record DebitOrder(
    UInt128 DebitAccountId,
    ulong Amount
);
