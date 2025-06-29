
namespace RetailBank.Services;


public interface ISimulationControllerService
{
    public void ToggleStart();
    public bool IsRunning { get; set; }
}