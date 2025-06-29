using TigerBeetle;

namespace RetailBank.Services;

public class TigerBeetleClientProvider : ITigerBeetleClientProvider
{
    private Client _client;

    public Client Client
    {
        get => _client;
        set
        {
            _client = value;
        }
    }

    public TigerBeetleClientProvider()
    {
        _client = InitialiseClient();
    }

    public Client InitialiseClient()
    {
        var tbAddress = Environment.GetEnvironmentVariable("TB_ADDRESS") ?? "4000";
        var clusterID = UInt128.Zero;
        var addresses = new[] { tbAddress };
        var client = new Client(clusterID, addresses);
        return client;
    }

    public void ResetClient()
    {
        _client = InitialiseClient(); 
    } 
}
