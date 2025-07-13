using Microsoft.Extensions.Options;
using RetailBank.Models.Options;

namespace RetailBank.Services;

public class SimulationControllerService(IOptions<SimulationOptions> options)
{
    public bool IsRunning { get; private set; } = false;
    public ulong UnixStartTime { get; private set; } = 1752419842000;
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
        var sim = (timestamp - UnixStartTime) * TimeScale + options.Value.SimulationStart;
        return sim;
    }
}
