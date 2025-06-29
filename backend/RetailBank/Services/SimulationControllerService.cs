namespace RetailBank.Services;

public class SimulationControllerService : ISimulationControllerService
{
    public bool IsRunning { get; set; } = false;
    public void ToggleStart()
    {
        IsRunning = !IsRunning;
    }
}