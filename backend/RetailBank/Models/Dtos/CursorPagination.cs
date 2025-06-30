namespace RetailBank.Models.Dtos;

public record CursorPagination<T>(
    IEnumerable<T> Items,
    string? Next
);
