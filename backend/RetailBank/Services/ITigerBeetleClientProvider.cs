using TigerBeetle;

namespace RetailBank.Services;

public interface ITigerBeetleClientProvider
{
    public Client Client { get; set; }
    public void ResetClient();
}
