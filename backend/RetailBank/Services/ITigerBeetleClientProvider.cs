using TigerBeetle;

namespace RetailBank.Services;

public interface ITigerBeetleClientProvider
{
    public void ResetClient();
    public Client Client { get; set; }

}