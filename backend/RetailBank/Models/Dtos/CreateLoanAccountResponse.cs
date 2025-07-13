using System.ComponentModel.DataAnnotations;
using RetailBank.Validation;

namespace RetailBank.Models.Dtos;

public record CreateLoanAccountResponse(
    [property: Required]
    [property: Length(13, 13)]
    [property: RegularExpression(ValidationConstants.Base10)]
    string AccountId
);
