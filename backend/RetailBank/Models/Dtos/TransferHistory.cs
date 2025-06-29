namespace RetailBank.Models.Dtos;

public record TransferHistory(
    IEnumerable<TransferEvent> TransferEvents
);
