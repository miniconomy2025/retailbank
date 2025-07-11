using System.Text.Json.Serialization;

namespace RetailBank.Models.Interbank;

public record CommercialAccountNumberResponse(
    [property: JsonPropertyName("account_number")]
    string AccountNumber
);
