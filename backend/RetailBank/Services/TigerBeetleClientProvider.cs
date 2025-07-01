using Microsoft.Extensions.Options;
using RetailBank.Models.Options;
using TigerBeetle;

namespace RetailBank.Services;

public class TigerBeetleClientProvider : ITigerBeetleClientProvider
{
    private Client _client;
    private string _tbAddress;

    public Client Client
    {
        get => _client;
        set
        {
            _client = value;
        }
    }

    public TigerBeetleClientProvider(IOptions<ConnectionStrings> options)
    {
        _tbAddress = options.Value.TigerBeetle;
        _client = InitialiseClient();
    }

    public Client InitialiseClient()
    {
        return new Client(0, [_tbAddress]);
    }

    public void ResetClient()
    {
        _client = InitialiseClient(); 
    } 
}
