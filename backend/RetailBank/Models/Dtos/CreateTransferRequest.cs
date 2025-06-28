namespace RetailBank.Models.Dtos;

public record CreateTransferRequest(ulong From, ulong To, string AmountCents);
