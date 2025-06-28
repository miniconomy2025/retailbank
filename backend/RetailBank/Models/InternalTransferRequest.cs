namespace RetailBank.Models;

public record InternalTransferRequest(UInt128 From, UInt128 To, UInt128 Amount);
