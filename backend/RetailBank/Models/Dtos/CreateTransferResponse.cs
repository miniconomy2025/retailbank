using System.ComponentModel.DataAnnotations;
using RetailBank.Validation;

namespace RetailBank.Models.Dtos;

public record CreateTransferResponse(
    [property: Required]
    [property: Length(32, 32)]
    [property: RegularExpression(ValidationConstants.Hex)]
    string TransferId
);
