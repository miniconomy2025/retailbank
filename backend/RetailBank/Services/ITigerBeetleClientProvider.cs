using TigerBeetle;

namespace RetailBank.Services;

public interface ITigerBeetleClientProvider
{
    Client Client { get; set; }
    void ResetClient();
}
