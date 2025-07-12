using Microsoft.Extensions.Options;
using RetailBank.Models.Options;
using TigerBeetle;

namespace RetailBank.Services;

public class TigerBeetleClientProvider
{
    private string _tbAddress;

    public Client Client { get; private set; }

    public TigerBeetleClientProvider(IOptions<ConnectionStrings> options)
    {
        _tbAddress = options.Value.TigerBeetle;
        Client = InitialiseClient();
    }

    public Client InitialiseClient()
    {
        return new Client(0, [_tbAddress]);
    }

    public void ResetClient()
    {
        Client = InitialiseClient(); 
    } 
}
