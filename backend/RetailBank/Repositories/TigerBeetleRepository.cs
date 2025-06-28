using System.Security.Permissions;
using RetailBank.Models;
using TigerBeetle;
namespace RetailBank.Repositories;

public class TigerBeetleRepository(Client tbClient) : ILedgerRepository
{

    public async Task CreateAccount(ulong accountNumber, UInt64 userData64 = 0, UInt128? userData128 = null, UInt32 userData32 = 0, uint ledger = 1, ushort code = 1, AccountFlags accountFlags = AccountFlags.None)
    {
        var account = new Account
        {
            Id = accountNumber,
            UserData128 = userData128 ?? 0,
            UserData64 = userData64,
            UserData32 = userData32,
            Ledger = ledger,
            Code = code,
            Flags = accountFlags,
        };

        var accountResult = await tbClient.CreateAccountAsync(account);
        if (accountResult != CreateAccountResult.Ok)
        {
            throw new TigerBeetleResultException<CreateAccountResult>(accountResult);
        }
    }


    public async Task Transfer(UInt128 id, ulong debitAccountId, ulong creditAccountId, UInt128 amount, uint ledger = 1, TransferFlags transferFlags = TransferFlags.None, ushort code = 1)
    {
        var transfer = new Transfer
        {
            Id = id,
            DebitAccountId = debitAccountId,
            CreditAccountId = creditAccountId,
            Amount = amount,
            Ledger = ledger,
            Flags = transferFlags,
            Code = code,
        };

        var transferResult = await tbClient.CreateTransferAsync(transfer);
        if (transferResult != CreateTransferResult.Ok)
        {
            throw new TigerBeetleResultException<CreateTransferResult>(transferResult);
        }
    }

    public async Task TransferAll(Transfer[] transfers)
    {
        var transferResults = await tbClient.CreateTransfersAsync(transfers);
        foreach (var result in transferResults)
        {
            if (result.Result != CreateTransferResult.Ok)
            {
                throw new TigerBeetleResultException<CreateTransferResult>(result.Result);
            }
        }
    }
}