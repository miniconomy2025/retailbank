namespace RetailBank.Models.Options;

public record SimulationOptions
{
    public const string Section = "Simulation";

    // seconds per day
    public ushort Period { get; init; }
}
