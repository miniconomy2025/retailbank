using System.ComponentModel.DataAnnotations;
using RetailBank.Validation;

namespace RetailBank.Models.Dtos;

public record CreateLoanAccountRequest(
    [property: Required]
    [property: Range(1, ulong.MaxValue)]
    ulong LoanAmountCents,
    [property: Required]
    [property: Length(12, 12)]
    [property: RegularExpression(ValidationConstants.Base10)]
    string DebtorAccountId
);
