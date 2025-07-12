using System.Text.Json.Serialization;

namespace RetailBank.Models.Interbank;

public record GetCommercialAccountBalanceResponse(
    [property: JsonPropertyName("balance")]
    decimal Balance
);
