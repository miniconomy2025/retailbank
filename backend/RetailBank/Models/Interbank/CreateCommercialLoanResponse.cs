using System.Text.Json.Serialization;

namespace RetailBank.Models.Interbank;

public record CreateCommercialLoanResponse(
    [property: JsonPropertyName("success")]
    bool Success,
    [property: JsonPropertyName("loan_number")]
    string LoanNumber
);
