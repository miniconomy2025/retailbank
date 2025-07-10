using Microsoft.Extensions.Options;
using RetailBank.Models.Options;

namespace RetailBank.Services;

public class SimulationControllerService(IOptions<SimulationOptions> options) : ISimulationControllerService
{
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
}
