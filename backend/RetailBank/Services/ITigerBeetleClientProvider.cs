using TigerBeetle;

namespace RetailBank.Services;

public interface ITigerBeetleClientProvider
{
    Client Client { get; }
    void ResetClient();
}
