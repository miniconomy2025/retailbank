using Microsoft.Extensions.Options;
using RetailBank.Models.Options;

namespace RetailBank.Services;

public class SimulationControllerService(IOptions<SimulationOptions> options) : ISimulationControllerService
{
    private const long InSimulationStart = 2524600800;

    public bool IsRunning { get; private set; } = false;
    public ulong StartTime { get; private set; } = 0;
    public uint TimeScale => options.Value.TimeScale;

    public void Start(ulong startTime)
    {
        IsRunning = true;
        StartTime = startTime;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    /// <summary>
    /// Map real nanosecond unix timestamp to in-sim seconds unix timestamp
    /// </summary>
    public static ulong MapToSimTimestamp(ulong timestamp, ulong startTime, uint timeScale)
    {
        return (ulong)long.Max(0, ((long)timestamp - (long)startTime) * (int)timeScale / 1_000_000_000 + InSimulationStart);
    }

    public static ulong SimDurationToRealDuration(ulong duration, uint timeScale)
    {
        return (ulong)long.Max(0, (long)duration / (int)timeScale);
    }
}
