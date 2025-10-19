namespace RetailBank.Models.Options;

public record SimulationOptions
{
    public const string Section = "Simulation";

    // simulated seconds per real life seconds
    // 1 rl day / 2 sim mins = 24*60*60 / 2*60 = 24*60 / 2 = 12*60 = 720
    public uint TimeScale { get; init; } = 720;
    public ulong SimulationStart { get; init; } = 2524600800000;
}
