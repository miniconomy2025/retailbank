namespace RetailBank.Models.Dtos;

public record CreateTransferResponse(
    string TransferId,
    CreationStatus CreationStatus
);
