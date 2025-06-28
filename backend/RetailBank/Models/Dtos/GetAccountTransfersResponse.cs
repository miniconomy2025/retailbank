namespace RetailBank.Models.Dtos;

public record GetAccountTransfersResponse(
    IEnumerable<TransferEvent> TransferEvents
);
