namespace RetailBank.Models;

public record ExternalTransferRequest(UInt128 From, string To, UInt128 Amount);
