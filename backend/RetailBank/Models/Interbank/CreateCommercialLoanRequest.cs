using System.Text.Json.Serialization;

namespace RetailBank.Models.Interbank;

public record CreateCommercialLoanRequest(
    [property: JsonPropertyName("amount")]
    decimal Amount
);
