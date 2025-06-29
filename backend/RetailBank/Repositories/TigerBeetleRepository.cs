using RetailBank.Models;
using TigerBeetle;

namespace RetailBank.Repositories;

public class TigerBeetleRepository(Client tbClient) : ILedgerRepository
{
    public async Task CreateAccount(ulong accountNumber, LedgerAccountCode code, ulong userData64 = 0, UInt128? userData128 = null, uint userData32 = 0, AccountFlags accountFlags = AccountFlags.None)
    {
        var account = new Account
        {
            Id = accountNumber,
            UserData128 = userData128 ?? 0,
            UserData64 = userData64,
            UserData32 = userData32,
            Ledger = 1,
            Code = (ushort)code,
            Flags = accountFlags,
        };

        var accountResult = await tbClient.CreateAccountAsync(account);
        if (accountResult != CreateAccountResult.Ok)
        {
            throw new TigerBeetleResultException<CreateAccountResult>(accountResult);
        }
    }

    public async Task Transfer(UInt128 transferId, ulong debitAccountId, ulong creditAccountId, UInt128 amount, TransferFlags transferFlags = TransferFlags.None, ushort code = 1, UInt64 userData64=0, UInt128? pendingId=null)
    {
        var transfer = new Transfer
        {
            Id = transferId,
            DebitAccountId = debitAccountId,
            CreditAccountId = creditAccountId,
            UserData64 = userData64,
            PendingId = pendingId ?? 0,
            Amount = amount,
            Ledger = 1,
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
