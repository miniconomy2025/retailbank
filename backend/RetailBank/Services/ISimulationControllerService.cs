namespace RetailBank.Services;

public interface ISimulationControllerService
{
    bool IsRunning { get; }
    ulong StartTime { get; }
    uint TimeScale { get; }
    
    void Start(ulong startTime);
    void Stop();
}
