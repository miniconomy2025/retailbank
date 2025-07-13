using System.ComponentModel.DataAnnotations;

namespace RetailBank.Models.Dtos;

public record CursorPagination<T>(
    [property: Required]
    IEnumerable<T> Items,
    [property: Required]
    [property: Url]
    string? Next
);
