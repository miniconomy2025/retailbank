namespace RetailBank.Models.Options;

public record ConnectionStrings
{
    public const string Section = "ConnectionStrings";

    public required string TigerBeetle { get; init; }
}
