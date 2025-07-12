using System.Text.Json.Serialization;

namespace RetailBank.Models.Interbank;

public record CreateCommercialTransferRequest(
    [property: JsonPropertyName("transaction_number")]
    string TransactionNumber,
    [property: JsonPropertyName("from_account_number")]
    string FromAccountNumber,
    [property: JsonPropertyName("to_account_number")]
    string ToAccountNumber,
    [property: JsonPropertyName("amount")]
    decimal Amount,
    [property: JsonPropertyName("description")]
    string Description
);
