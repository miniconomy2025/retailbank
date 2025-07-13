using System.ComponentModel.DataAnnotations;
using RetailBank.Validation;

namespace RetailBank.Models.Dtos;

public record CreateTransactionalAccountResponse(
    [property: Required]
    [property: Length(12, 12)]
    [property: RegularExpression(ValidationConstants.Base10)]
    string AccountId
);
