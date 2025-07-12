using System.Text.Json.Serialization;

namespace RetailBank.Models.Interbank;

public record GetCommercialAccountResponse(
    [property: JsonPropertyName("account_number")]
    string AccountNumber,
    [property: JsonPropertyName("net_balance")]
    decimal NetBalance
);
