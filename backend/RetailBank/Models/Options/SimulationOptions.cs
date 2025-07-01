namespace RetailBank.Models.Options;

public record SimulationOptions
{
    public const string Section = "Simulation";

    // seconds per day
    public required ushort Period { get; init; }
}
