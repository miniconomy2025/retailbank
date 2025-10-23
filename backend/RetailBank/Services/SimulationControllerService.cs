using Microsoft.Extensions.Options;
using RetailBank.Models.Options;

namespace RetailBank.Services;

public class SimulationControllerService(IOptions<SimulationOptions> options)
{
    public bool IsRunning { get; private set; } = false;
    public ulong UnixStartTime { get; private set; }
    public uint TimeScale => options.Value.TimeScale;

    public void Start(ulong startTime)
    {
        IsRunning = true;
        UnixStartTime = startTime;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public ulong TimestampToSim(ulong timestamp)
    {
        var sim = ((long)timestamp - (long)UnixStartTime) * TimeScale + (long)options.Value.SimulationStart;
        sim = long.Clamp(sim, -62135596800000L, 253402300799999L);
        return (ulong)sim;
    }
}
