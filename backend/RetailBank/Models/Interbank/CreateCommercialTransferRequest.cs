using System.Text.Json.Serialization;

namespace RetailBank.Models.Interbank;

public record CreateCommercialTransferRequest(
    [property: JsonPropertyName("to_account_number")]
    string ToAccountNumber,
    [property: JsonPropertyName("to_bank_name")]
    string ToBankName,
    [property: JsonPropertyName("amount")]
    decimal Amount,
    [property: JsonPropertyName("description")]
    string Description
);
