namespace RetailBank.Models;

public record ExternalTransferRequest(UInt128 From, UInt128 To, UInt128 Amount);
