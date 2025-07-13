using System.ComponentModel.DataAnnotations;

namespace RetailBank.Models.Dtos;

public record CreateTransactionAccountRequest(
    [property: Required]
    [property: Range(0, ulong.MaxValue)]
    ulong SalaryCents
);
