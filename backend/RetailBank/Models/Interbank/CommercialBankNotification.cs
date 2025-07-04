using System.Text.Json.Serialization;

namespace RetailBank.Models.Interbank;

public record CommercialBankNotification(
    [property: JsonPropertyName("transaction_number")]
    string TransactionNumber,
    [property: JsonPropertyName("from_account_number")]
    string FromAccountNumber,
    [property: JsonPropertyName("to_account_number")]
    string ToAccountNumber,
    [property: JsonPropertyName("amount")]
    UInt128 Amount,
    [property: JsonPropertyName("description")]
    string Description
);
